// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

[AllowAnonymous]
public class UpdateEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    private readonly ILogger<UpdateEmailModel> _logger;

    public UpdateEmailModel(
        UserManager<User> userManager,
        IEmailSender emailSender,
        ILogger<UpdateEmailModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public string Email { get; private set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }

    private void LoadEmail()
    {
        Email = (string)TempData.Peek("Email");
    }

    private void LoadInput()
    {
        LoadEmail();
        Input = new InputModel
        {
            NewEmail = Email
        };
    }

    private void SetNewEmail()
    {
        TempData["Email"] = Input.NewEmail;
    }

    public IActionResult OnGet()
    {
        LoadInput();

        if (string.IsNullOrWhiteSpace(Email))
        {
            _logger.LogWarning("Unable to load user email.");
            RedirectToPage("./Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadEmail();

        if (string.IsNullOrWhiteSpace(Email))
        {
            _logger.LogWarning("Unable to load user email.");
            RedirectToPage("./Login");
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user is null)
            return NotFound($"Unable to load user with email '{Email}'.");

        if (!ModelState.IsValid)
        {
            LoadInput();
            return Page();
        }

        if (!string.Equals(Input.NewEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var result = await _userManager.UpdateEmailAsync(user, Input.NewEmail);
            if (!result.Succeeded)
            {
                //Reveal details about account state only if provided credentials are valid
                if (!result.Errors.Contains(IdentityError.InvalidCredentials))
                {
                    if (result.Errors.Contains(IdentityError.AccountSuspended))
                    {
                        _logger.LogWarning("User account suspended.");
                        TempData["SuspendedUntil"] = user.SuspendedUntil.Value;
                        return RedirectToPage("./Suspension");
                    }
                    else if (result.Errors.Contains(IdentityError.AccountLockedOut))
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else if (result.Errors.Contains(IdentityError.EmailDuplicated))
                    {
                        ModelState.AddModelError(string.Empty, "Provided email is already in use.");
                        return Page();
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid email change attempt.");
                return Page();
            }

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, email = Input.NewEmail, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.NewEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Confirmation link to change email sent. Please check your email.";
            SetNewEmail();
            return RedirectToPage("./Login");
        }

        ModelState.AddModelError(string.Empty, "Your email is unchanged.");
        return RedirectToPage();
    }
}
