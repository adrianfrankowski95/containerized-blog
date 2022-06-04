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
using Blog.Services.Identity.API.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ISignInManager<User> _signInManager;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly ISysTime _sysTime;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailModel> _logger;

    public EmailModel(
        UserManager<User> userManager,
        ISignInManager<User> signInManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        ISysTime sysTime,
        IEmailSender emailSender,
        ILogger<EmailModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailOptions = emailOptions;
        _sysTime = sysTime;
        _emailSender = emailSender;
        _logger = logger;
    }

    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public InputModel Input { get; set; }


    public class InputModel
    {
        [Display(Name = "Current email")]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        [Unlike("Email", ErrorMessage = "The {0} and {1} must be different.")]
        public string NewEmail { get; set; }
    }

    private void LoadInput(User user)
    {
        Input = new InputModel
        {
            Email = user.Email,
            NewEmail = user.Email
        };

        IsEmailConfirmed = !_userManager.IsConfirmingEmail(user);
    }

    private void SaveEmail(User user)
    {
        TempData["Email"] = user.Email;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        LoadInput(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            LoadInput(user);
            return Page();
        }

        if (!string.Equals(Input.NewEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var result = await _userManager.UpdateEmailAsync(user, Input.NewEmail);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(x => x is EmailValidationError))
                {
                    foreach (var error in result.Errors.Where(x => x is EmailValidationError))
                    {
                        ModelState.AddModelError(string.Empty, error.ErrorDescription);
                    }
                    return Page();
                }
                else if (result.Errors.Any(x => x is UsernameValidationError))
                {
                    _logger.LogWarning("User username does not meet validation requirements anymore.");
                    SaveEmail(user);
                    return RedirectToPage("./UpdateUsername", new { returnUrl = Url.Page("./Email") });
                }

                ModelState.AddModelError(string.Empty, "Invalid email change attempt.");
                return Page();
            }

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, email = Input.NewEmail, code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                 Input.NewEmail,
                 "Confirm your email",
                 $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>." +
                 $"<br><br>This link will expire at {_sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod)).ToString("dddd, dd mmmm yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo)}.");

            StatusMessage = "Confirmation link to change email sent. Please check your email.";

            if (!_emailOptions.CurrentValue.RequireConfirmed)
                await _signInManager.RefreshSignInAsync(HttpContext, user);

            _logger.LogInformation("User changed their email successfully.");
            StatusMessage = "Your email has been changed.";
            return RedirectToPage();
        }

        ModelState.AddModelError(null, "The New email and Current email must be different.");
        return Page();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            LoadInput(user);
            return Page();
        }

        var result = await _userManager.GenerateEmailConfirmationAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email verification send attempt.");
            return Page();
        }

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { userId = user.Id, code },
            protocol: Request.Scheme);
        // await _emailSender.SendEmailAsync(
        //     user.Email,
        //     "Confirm your email",
        //     $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }
}
