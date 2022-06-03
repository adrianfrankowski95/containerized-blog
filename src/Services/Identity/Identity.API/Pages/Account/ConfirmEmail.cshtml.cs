// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ConfirmEmailModel> _logger;

    public ConfirmEmailModel(UserManager<User> userManager, ILogger<ConfirmEmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }


    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid userId, Guid code)
    {
        if (userId == default || code == default)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
        {
            if (result.Errors.Single().Equals(EmailConfirmationError.ExpiredEmailConfirmationCode))
            {
                return RedirectToPage("./EmailConfirmationExpiration");
            }
            else if (result.Errors.Single() is EmailConfirmationError)
            {
                ModelState.AddModelError(string.Empty, result.Errors.Single().ErrorDescription);
                return Page();
            }
            else if (result.Errors.All(x => x is UsernameValidationError or EmailValidationError))
            {
                if (result.Errors.Any(x => x is UsernameValidationError))
                {
                    _logger.LogWarning("User username does not meet validation requirements anymore.");
                    return RedirectToPage("./UpdateUsername",
                        new { returnUrl = Url.Page("./ConfirmEmail", new { userId, code }) });
                }
                else if (result.Errors.Any(x => x is EmailValidationError))
                {
                    _logger.LogWarning("User email does not meet validation requirements anymore.");
                    return RedirectToPage("./UpdateEmail",
                        new { returnUrl = Url.Page("./ConfirmEmail", new { userId, code }) });
                }
            }

            StatusMessage = "Error confirming your email.";
        }
        else
        {
            StatusMessage = "Thank you for confirming your email.";
        }

        return Page();
    }
}
