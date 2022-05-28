// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public class UpdateUsernameModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UpdateUsernameModel> _logger;

    public UpdateUsernameModel(
        UserManager<User> userManager,
        ILogger<UpdateEmailModel> logger)
    {
        _userManager = userManager;
    }

    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "New username")]
        public string NewUsername { get; set; }
    }

    private string LoadEmail() => (string)TempData.Peek("Email");

    private void LoadInput(User user)
    {
        Username = user.Username;

        Input = new InputModel
        {
            NewUsername = user.Username,
        };
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return NotFound($"Unable to load user email.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        LoadInput(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var email = LoadEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Unable to load user email.");
            LocalRedirect(ReturnUrl);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return NotFound($"Unable to load user with email '{email}'.");

        if (!ModelState.IsValid)
        {
            LoadInput(user);
            return Page();
        }

        if (!string.Equals(Input.NewUsername, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            var result = await _userManager.UpdateUsernameAsync(user, Input.NewUsername);
            if (!result.Succeeded)
            {
                //Reveal details about account state only if provided credentials are valid
                if (!result.Errors.Contains(IdentityError.InvalidCredentials))
                {
                    if (result.Errors.Contains(IdentityError.AccountSuspended))
                    {
                        _logger.LogWarning("User account suspended.");
                        return RedirectToPage("./Suspension", new { suspendedUntil = user.SuspendedUntil.Value });
                    }
                    else if (result.Errors.Contains(IdentityError.AccountLockedOut))
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else if (result.Errors.Contains(IdentityError.UsernameDuplicated))
                    {
                        ModelState.AddModelError(string.Empty, "The Username is already in use.");
                        return Page();
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid username update attempt.");
                return Page();
            }

            StatusMessage = "Your username has been changed.";
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Your username is unchanged.");
        return RedirectToPage();
    }
}
