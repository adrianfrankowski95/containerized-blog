// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class SuspensionModel : PageModel
{
    [TempData]
    public Instant SuspendedUntil { get; set; }
    public void OnGet()
    {
    }
}
