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

    public ResetPasswordModel(UserManager<User> userManager)
    {
        _userManager = userManager;
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
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public async Task<IActionResult> OnGet(Guid userId, string code = null)
    {
        if (userId == null || userId == default)
        {
            return BadRequest("A user ID must be supplied for password reset.");
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        // Don't reveal that the user does not exist
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            if (_userManager.IsPasswordResetCodeExpired(user))
                return RedirectToPage("./ResetPasswordExpiration");
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
        if (user is null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.NewPassword, Input.Code);
        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        if (result.Errors.Count == 1 && result.Errors.Contains(IdentityError.ExpiredPasswordResetCode))
        {
            return RedirectToPage("./ResetPasswordExpiration");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.ErrorDescription);
        }
        return Page();
    }
}
