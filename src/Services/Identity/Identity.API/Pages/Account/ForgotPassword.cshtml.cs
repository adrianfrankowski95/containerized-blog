// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
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
    private readonly ISysTime _sysTime;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(
        UserManager<User> userManager,
        IOptionsMonitor<PasswordOptions> passwordOptions,
        ISysTime sysTime,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _passwordOptions = passwordOptions;
        _sysTime = sysTime;
    }


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
            if (result.Succeeded)
            {
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.PasswordResetCode));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>." +
                    $"<br><br>This link will expire at {_sysTime.Now.Plus(Duration.FromTimeSpan(_passwordOptions.CurrentValue.PasswordResetCodeValidityPeriod)).ToString("dddd, dd mmmm yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo)}.");

            }
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
