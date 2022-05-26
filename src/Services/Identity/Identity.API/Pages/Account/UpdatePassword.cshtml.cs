// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

[AllowAnonymous]
public class UpdatePassword : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ChangePasswordModel> _logger;

    public UpdatePassword(
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
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    private string LoadEmail() => (string)TempData.Peek("Email");

    public IActionResult OnGet()
    {
        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Unable to load user email.");
            RedirectToPage("./Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Unable to load user email.");
            RedirectToPage("./Login");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        if (!ModelState.IsValid)
            return Page();

        var changePasswordResult = await _userManager.UpdatePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorDescription);
            }

            return Page();
        }

        StatusMessage = "Your password has been changed, please re-login.";
        return RedirectToPage("./Login");
    }
}
