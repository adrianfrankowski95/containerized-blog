// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure.Validation;
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

public class UpdateEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IRequestClient<SendEmailConfirmationEmailRequest> _emailSender;
    private readonly ISysTime _sysTime;
    private readonly ILogger<UpdateEmailModel> _logger;

    public UpdateEmailModel(
        UserManager<User> userManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        IRequestClient<SendEmailConfirmationEmailRequest> emailSender,
        ISysTime sysTime,
        ILogger<UpdateEmailModel> logger)
    {
        _userManager = userManager;
        _emailOptions = emailOptions;
        _sysTime = sysTime;
        _emailSender = emailSender;
        _logger = logger;
    }

    public string ReturnUrl { get; set; }

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

    private string LoadEmail()
        => (string)TempData.Peek("Email");


    private void LoadInput()
    {
        string email = LoadEmail();
        Input = new InputModel
        {
            Email = email,
            NewEmail = email
        };
    }

    private void SetNewEmail()
    {
        TempData["Email"] = Input.NewEmail;
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        LoadInput();

        if (string.IsNullOrWhiteSpace(Input.Email))
        {
            return NotFound($"Unable to load user email.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        string email = LoadEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            return NotFound($"Unable to load user email.");
        }

        if (!ModelState.IsValid)
        {
            LoadInput();
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        if (!string.Equals(Input.NewEmail, user.EmailAddress, StringComparison.OrdinalIgnoreCase))
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
                    return RedirectToPage("./UpdateUsername", new { returnUrl = Url.Page("./UpdateEmail", new { returnUrl }) });
                }

                ModelState.AddModelError(string.Empty, "Invalid email update attempt.");
                return Page();
            }

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

            StatusMessage = "Confirmation link to change email sent. Please check your email.";
            SetNewEmail();
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "The New email and Current email must be different.");
        return Page();
    }
}
