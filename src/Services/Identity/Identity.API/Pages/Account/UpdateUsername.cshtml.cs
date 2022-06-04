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

    [TempData]
    public string StatusMessage { get; set; }
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        public string Username { get; set; }

        [BindProperty]
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "New username")]
        [Unlike("Username", ErrorMessage = "The {0} and {1} must be different.")]
        public string NewUsername { get; set; }
    }

    private string LoadEmail() => (string)TempData.Peek("Email");

    private void LoadInput(User user)
    {
        Input = new InputModel
        {
            Username = user.Username,
            NewUsername = user.Username,
        };
    }

    private void SaveEmail(User user)
    {
        TempData["Email"] = user.Email;
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

        if (!string.Equals(Input.NewUsername, user.Username, StringComparison.Ordinal))
        {
            var result = await _userManager.UpdateUsernameAsync(user, Input.NewUsername);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(x => x is UsernameValidationError))
                {
                    foreach (var error in result.Errors.Where(x => x is UsernameValidationError))
                    {
                        ModelState.AddModelError(string.Empty, error.ErrorDescription);
                    }
                    return Page();
                }
                else if (result.Errors.Any(x => x is EmailValidationError))
                {
                    _logger.LogWarning("User email does not meet validation requirements anymore.");
                    SaveEmail(user);
                    return RedirectToPage("./UpdateEmail", new { returnUrl = Url.Page("./UpdateUsername", new { returnUrl }) });
                }

                ModelState.AddModelError(string.Empty, "Invalid username update attempt.");
                return Page();
            }

            StatusMessage = "Your username has been updated.";
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "The New username and Old username must be different.");
        return Page();
    }
}
