// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ISignInManager<User> _signInManager;
    private readonly ILoginService<User> _loginService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(ISignInManager<User> signInManager, ILoginService<User> loginService, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _loginService = loginService;
        _logger = logger;
    }


    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    private void LoadInput()
    {
        Input = new InputModel()
        {
            Email = (string)TempData[nameof(Input.Email)],
            RememberMe = (bool?)TempData[nameof(Input.RememberMe)] ?? false
        };
    }

    private void SaveInput()
    {
        TempData[nameof(Input.Email)] = Input.Email;
        TempData[nameof(Input.RememberMe)] = Input.RememberMe;
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
            ModelState.AddModelError(string.Empty, ErrorMessage);

        ReturnUrl = returnUrl ?? Url.Content("~/");

        await _signInManager.SignOutAsync(HttpContext);

        LoadInput();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            (IdentityResult result, User user) = await _loginService.LoginAsync(Input.Email, Input.Password);
            if (!result.Succeeded)
            {
                if (result.Errors.Single().Equals(CredentialsError.InvalidCredentials))
                {
                    ModelState.AddModelError(string.Empty, CredentialsError.InvalidCredentials.ErrorDescription);
                    return Page();
                }
                else if (result.Errors.Single().Equals(UserStateValidationError.AccountLockedOut))
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else if (result.Errors.Single().Equals(UserStateValidationError.AccountSuspended))
                {
                    _logger.LogWarning("User account suspended.");
                    return RedirectToPage("./Suspension", new { suspendedUntil = user.SuspendedUntil.Value });
                }
                else if (result.Errors.Single().Equals(UserStateValidationError.EmailUnconfirmed))
                {
                    _logger.LogWarning("User email address has not been confirmed.");
                    return RedirectToPage("./UnconfirmedEmail");
                }
                else if (result.Errors.Single().Equals(UserStateValidationError.ResettingPassword))
                {
                    _logger.LogWarning("User password has not been confirmed.");
                    return RedirectToPage("./UnconfirmedPassword");
                }
                else if (result.Errors.All(x => x is UsernameValidationError or EmailValidationError or PasswordValidationError))
                {
                    SaveInput();
                    if (result.Errors.Any(x => x is UsernameValidationError))
                    {
                        _logger.LogWarning("User username does not meet validation requirements anymore.");
                        return RedirectToPage("./UpdateUsername",
                            new { returntUrl = Url.Page("./Login", new { returnUrl }) });
                    }
                    else if (result.Errors.Any(x => x is EmailValidationError))
                    {
                        _logger.LogWarning("User email does not meet validation requirements anymore.");
                        return RedirectToPage("./UpdateEmail",
                            new { returntUrl = Url.Page("./Login", new { returnUrl }) });
                    }
                    else if (result.Errors.Any(x => x is PasswordValidationError))
                    {
                        _logger.LogWarning("User password does not meet validation requirements anymore.");
                        return RedirectToPage("./UpdatePassword",
                            new { returntUrl = Url.Page("./Login", new { returnUrl }) });
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            _logger.LogInformation("User logged in.");
            await _signInManager.SignInAsync(HttpContext, user, Input.RememberMe);
            return LocalRedirect(returnUrl);
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
