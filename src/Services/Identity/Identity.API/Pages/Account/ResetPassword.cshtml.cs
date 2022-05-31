// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
namespace Blog.Services.Identity.API.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(UserManager<User> userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }


    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }

    private void SaveEmail(User user)
    {
        TempData[nameof(user.Email)] = user.Email;
    }

    private void SaveUsername(User user)
    {
        TempData[nameof(user.Username)] = user.Username;
    }

    public IActionResult OnGet(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        Input = new InputModel
        {
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null || _userManager.IsConfirmingEmail(user))
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.NewPassword, Input.Code);
        if (!result.Succeeded)
        {
            if (result.Errors.Contains(PasswordResetError.ExpiredPasswordResetCode))
            {
                return RedirectToPage("./ResetPasswordExpiration");
            }

            //Reveal details about account state only if provided credentials are valid
            if (!result.Errors.Contains(CredentialsError.InvalidCredentials))
            {
                if (result.Errors.Contains(UserStateValidationError.AccountSuspended))
                {
                    _logger.LogWarning("User account suspended.");
                    return RedirectToPage("./Suspension", new { suspendedUntil = user.SuspendedUntil.Value });
                }
                else if (result.Errors.Contains(UserStateValidationError.AccountLockedOut))
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else if (result.Errors.Contains(UsernameValidationError.InvalidUsernameFormat))
                {
                    _logger.LogWarning("User username does not meet validation requirements anymore.");
                    SaveUsername(user);
                    return RedirectToPage("./UpdateUsername",
                        new { returnUrl = Url.Page("./ResetPassword.cshtml", new { code = Input.Code }) });
                }
                else if (result.Errors.Contains(EmailValidationError.InvalidEmailFormat))
                {
                    _logger.LogWarning("User email does not meet validation requirements anymore.");
                    SaveEmail(user);
                    return RedirectToPage("./UpdateEmail",
                        new { returnUrl = Url.Page("./ResetPassword.cshtml", new { code = Input.Code }) });
                }
            }

            foreach (var error in result.Errors.Where(x => x is EmailValidationError || x is PasswordValidationError))
            {
                ModelState.AddModelError(string.Empty, error.ErrorDescription);
            }
            return Page();
        }

        return RedirectToPage("./ResetPasswordConfirmation");
    }
}
