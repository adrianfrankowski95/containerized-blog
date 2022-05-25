// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public class UpdateEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly ILogger<UpdateEmailModel> _logger;

    public UpdateEmailModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        IEmailSender emailSender,
        ILogger<UpdateEmailModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailOptions = emailOptions;
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

    private void Load(User user)
    {
        Email = user.Email;

        Input = new InputModel
        {
            NewEmail = user.Email,
        };
    }

    public async Task<IActionResult> OnGetAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound($"Unable to load user with ID '{userId}'.");

        Load(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.FindByEmailAsync(Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            Load(user);
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
                        if (user is not null)
                            return RedirectToPage("./Suspension", new { userId = user.Id });
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
            return RedirectToPage("./Login");
        }

        ModelState.AddModelError(string.Empty, "Your email is unchanged.");
        return RedirectToPage();
    }
}
