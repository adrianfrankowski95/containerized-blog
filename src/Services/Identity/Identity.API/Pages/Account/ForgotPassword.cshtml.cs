// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Blog.Services.Identity.API.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }


    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null || _userManager.IsConfirmingEmail(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user);
            if (result.Succeeded)
            {
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.PasswordResetCode));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
