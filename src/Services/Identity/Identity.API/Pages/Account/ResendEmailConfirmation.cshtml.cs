// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Blog.Services.Messaging.Requests;
using Blog.Services.Messaging.Responses;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IRequestClient<SendEmailConfirmationEmailRequest> _emailSender;
    private readonly ISysTime _sysTime;

    public ResendEmailConfirmationModel(
        UserManager<User> userManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        IRequestClient<SendEmailConfirmationEmailRequest> emailSender,
        ISysTime sysTime)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _emailOptions = emailOptions;
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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }

        var result = await _userManager.GenerateEmailConfirmationAsync(user);

        // Don't reveal any validation details
        if (result.Succeeded)
        {
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, code },
                protocol: Request.Scheme);

            var response = await _emailSender.GetResponse<SendEmailConfirmationEmailResponse>(
                    new(Username: user.FullName,
                        EmailAddress: user.EmailAddress,
                        CallbackUrl: callbackUrl,
                        UrlValidUntil: _sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod))));

            if (!response.Message.Success)
            {
                StatusMessage = "Error sending an email. Please try again later.";
                return RedirectToPage();
            }

            // await _emailSender.SendEmailAsync(
            //     Input.Email,
            //     "Confirm your email",
            //     $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>." +
            //     $"<br><br>This link will expire at {_sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod)).ToString("dddd, dd mmmm yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo)}.");
        }

        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        return Page();
    }
}
