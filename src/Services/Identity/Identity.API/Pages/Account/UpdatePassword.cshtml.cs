// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

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

    private void SaveUserId(User user)
    {
        TempData["UserId"] = user.Id;
    }

    private Guid GetUserId()
        => (Guid)TempData["UserId"];

    private void KeepUserId()
    {
        TempData.Keep("UserId");
    }

    public async Task<IActionResult> OnGetAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        SaveUserId(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = GetUserId();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        if (!ModelState.IsValid)
        {
            KeepUserId();
            return Page();
        }

        var changePasswordResult = await _userManager.UpdatePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorDescription);
            }
            KeepUserId();
            return Page();
        }

        ///Login with TempData from Login.cshtml.cs!
        StatusMessage = "Your password has been changed, please re-login.";
        return RedirectToPage("./Login");
    }
}
