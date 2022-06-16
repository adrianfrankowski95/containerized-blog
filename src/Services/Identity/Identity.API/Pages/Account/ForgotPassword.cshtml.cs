// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Messaging.Requests;
using Blog.Services.Messaging.Responses;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IOptionsMonitor<PasswordOptions> _passwordOptions;
    private readonly IRequestClient<SendPasswordResetEmailRequest> _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;
    private readonly ISysTime _sysTime;

    public ForgotPasswordModel(
        UserManager<User> userManager,
        IOptionsMonitor<PasswordOptions> passwordOptions,
        IRequestClient<SendPasswordResetEmailRequest> emailSender,
        ILogger<ForgotPasswordModel> logger,
        ISysTime sysTime)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _passwordOptions = passwordOptions;
        _logger = logger;
        _sysTime = sysTime;
    }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null || _userManager.IsConfirmingEmail(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user);

            // Don't reveal any validation details
            if (result.Succeeded)
            {
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.PasswordResetCode));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                var response = await _emailSender.GetResponse<SendPasswordResetEmailResponse>(
                    new(Username: user.FullName,
                        EmailAddress: user.EmailAddress,
                        CallbackUrl: callbackUrl,
                        UrlValidUntil: _sysTime.Now.Plus(Duration.FromTimeSpan(_passwordOptions.CurrentValue.PasswordResetCodeValidityPeriod))));

                if (!response.Message.Success)
                {
                    StatusMessage = "Error sending an email. Please try again later.";
                    return RedirectToPage();
                }
            }
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
