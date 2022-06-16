// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ISignInManager<User> _signInManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        UserManager<User> userManager,
        ISignInManager<User> signInManager,
        ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }


    public string Username { get; set; }


    [TempData]
    public string StatusMessage { get; set; }


    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at at max {1} characters long.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "I would like to receive additional updates and announcements via email")]
        public bool ReceiveAdditionalEmails { get; set; }
    }

    private void Load(User user)
    {
        Username = user.Username;

        Input = new InputModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ReceiveAdditionalEmails = user.ReceiveAdditionalEmails
        };
    }

    private void SaveEmail(User user)
    {
        TempData["Email"] = user.EmailAddress;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Load(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            Load(user);
            return Page();
        }

        bool isChanged = false;
        if (!string.Equals(Input.FirstName, user.FirstName, StringComparison.Ordinal))
        {
            user.FirstName = Input.FirstName;
            isChanged = true;
        }
        if (!string.Equals(Input.LastName, user.LastName, StringComparison.Ordinal))
        {
            user.LastName = Input.LastName;
            isChanged = true;
        }
        if (!Input.ReceiveAdditionalEmails != user.ReceiveAdditionalEmails)
        {
            user.ReceiveAdditionalEmails = Input.ReceiveAdditionalEmails;
            isChanged = true;
        }

        if (isChanged)
        {
            var result = await _userManager.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                if (result.Errors.All(x => x is UsernameValidationError or EmailValidationError))
                {
                    SaveEmail(user);
                    object returnRoute = new { returntUrl = Url.Page("./Index") };

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

                StatusMessage = "Unexpected error when trying to update the profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated.";
            return RedirectToPage();
        }

        StatusMessage = "No changes have been made.";
        return RedirectToPage();
    }
}
