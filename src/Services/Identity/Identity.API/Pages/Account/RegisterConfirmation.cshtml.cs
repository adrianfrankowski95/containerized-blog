// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _sender;

    public RegisterConfirmationModel(UserManager<User> userManager, IEmailSender sender)
    {
        _userManager = userManager;
        _sender = sender;
    }


    public string Email { get; set; }


    public bool DisplayConfirmAccountLink { get; set; }


    public string EmailConfirmationUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Index");
        }
        returnUrl ??= Url.Content("~/");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        Email = email;
        // Once you add a real email sender, you should remove this code that lets you confirm the account
        DisplayConfirmAccountLink = true;
        if (DisplayConfirmAccountLink)
        {

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);
        }

        return Page();
    }
}
