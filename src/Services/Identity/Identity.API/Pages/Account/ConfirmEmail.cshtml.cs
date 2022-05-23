// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Blog.Services.Identity.API.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public ConfirmEmailModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
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
        StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
        return Page();
    }
}
