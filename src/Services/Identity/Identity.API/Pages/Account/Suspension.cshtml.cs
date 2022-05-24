// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public Instant SuspendedUntil { get; private set; }
    public IActionResult OnGet(Instant? suspendedUntil)
    {
        if (suspendedUntil is null || suspendedUntil == default)
            return Redirect(Request.Headers["Referer"]);

        SuspendedUntil = suspendedUntil.Value;

        return Page();
    }
}
