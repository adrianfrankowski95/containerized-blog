// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable


using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILoginService<User> _loginService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<User> signInManager, ILoginService<User> loginService, ILogger<LoginModel> logger)
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
            RememberMe = (bool)TempData[nameof(Input.RememberMe)]
        };
    }

    private void SaveInput()
    {
        TempData[nameof(Input.Email)] = Input.Email;
        TempData[nameof(Input.Password)] = Input.Password;
        TempData[nameof(Input.RememberMe)] = Input.RememberMe;
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (_signInManager.IsSignedIn(User))
            return LocalRedirect(ReturnUrl);

        LoadInput();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _loginService.LoginAsync(Input.Email, Input.Password, out User user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                await _signInManager.SignInAsync(HttpContext, user, Input.RememberMe);

                return LocalRedirect(returnUrl);
            }

            //Reveal details about account state only if provided credentials are valid
            if (!result.Errors.Contains(IdentityError.InvalidCredentials))
            {
                if (result.Errors.Contains(IdentityError.AccountSuspended))
                {
                    _logger.LogWarning("User account suspended.");

                    if (user is not null)
                        return RedirectToPage("./Suspension", new { userId = user.Id });

                }
                else if (result.Errors.Contains(IdentityError.AccountLockedOut))
                {
                    _logger.LogWarning("User account locked out.");

                    return RedirectToPage("./Lockout");
                }
                else if (result.Errors.Contains(IdentityError.InvalidUsernameFormat))
                {
                    _logger.LogWarning("User username does not meet validation requirements anymore.");

                    SaveInput();

                    return RedirectToPage("./UpdateUsername", new { userId = user.Id });
                }
                else if (result.Errors.Contains(IdentityError.EmailDuplicated) ||
                        result.Errors.Contains(IdentityError.InvalidEmailFormat))
                {
                    _logger.LogWarning("User email does not meet validation requirements anymore.");
                    return RedirectToPage("./UpdateEmail", new { userId = user.Id });
                }
                else if (result.Errors.Contains(IdentityError.PasswordTooShort) ||
                        result.Errors.Contains(IdentityError.PasswordWithoutDigit) ||
                        result.Errors.Contains(IdentityError.PasswordWithoutLowerCase) ||
                        result.Errors.Contains(IdentityError.PasswordWithoutNonAlphanumeric) ||
                        result.Errors.Contains(IdentityError.PasswordWithoutUpperCase))
                {
                    _logger.LogWarning("User password does not meet validation requirements anymore.");

                    SaveInput();

                    return RedirectToPage("./UpdatePassword", new { userId = user.Id });
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
