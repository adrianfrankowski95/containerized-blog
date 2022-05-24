// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

/// <summary>
///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
[AllowAnonymous]
public class SuspensionModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SuspensionModel> _logger;
    public SuspensionModel(UserManager<User> userManager, ILogger<SuspensionModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public Instant SuspendedUntil { get; private set; }
    public async Task<IActionResult> OnGetAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound($"Unable to load user with ID '{userId}'.");

        if (!_userManager.IsSuspended(user))
        {
            _logger.LogWarning("User is not suspended.");
            return RedirectToPage("/Index");
        }

        SuspendedUntil = user.SuspendedUntil!.Value;

        return Page();
    }
}
