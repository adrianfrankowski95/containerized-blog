// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public class UpdatePasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ChangePasswordModel> _logger;

    public UpdatePasswordModel(
        UserManager<User> userManager,
        ILogger<ChangePasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [Unlike("Old Password", ErrorMessage = "The {0} and {1} must be different.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The New Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    private string LoadEmail() => (string)TempData.Peek("Email");

    private void SaveEmail(User user)
    {
        TempData[nameof(user.Email)] = user.Email;
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return NotFound($"Unable to load user email.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return NotFound($"Unable to load user email.");
        }

        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        if (!string.Equals(Input.NewPassword, Input.OldPassword, StringComparison.Ordinal))
        {
            var result = await _userManager.UpdatePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Single().Equals(CredentialsError.InvalidCredentials))
                {
                    ModelState.AddModelError(string.Empty, "The Old password is invalid.");
                    return Page();
                }
                else if (result.Errors.Any(x => x is PasswordValidationError))
                {
                    foreach (var error in result.Errors.Where(x => x is EmailValidationError))
                    {
                        ModelState.AddModelError(string.Empty, error.ErrorDescription);
                    }
                    return Page();
                }
                else if (result.Errors.All(x => x is UsernameValidationError or EmailValidationError))
                {
                    SaveEmail(user);
                    object returnRoute = new { returntUrl = Url.Page("./UpdatePassword", new { returnUrl }) };

                    if (result.Errors.Any(x => x is UsernameValidationError))
                    {
                        _logger.LogWarning("User username does not meet validation requirements anymore.");
                        return RedirectToPage("./UpdateUsername", returnRoute);
                    }
                    else if (result.Errors.Any(x => x is EmailValidationError))
                    {
                        _logger.LogWarning("User email does not meet validation requirements anymore.");
                        return RedirectToPage("./UpdateEmail", returnRoute);
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid password update attempt.");
                return Page();
            }

            StatusMessage = "Your password has been changed.";
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "The New password and Old password must be different.");
        return Page();
    }
}
