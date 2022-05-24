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

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    private void Load(string email, bool rememberMe, string returnUrl)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        Input = new InputModel
        {
            Email = email,
            RememberMe = rememberMe
        };
    }

    public async Task OnGetAsync(string email = null, bool rememberMe = false, string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        Load(email, rememberMe, returnUrl);

        // Clear the existing external cookie to ensure a clean login process
        await _signInManager.SignOutAsync(HttpContext);
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
                else if (result.Errors.Contains(IdentityError.EmailDuplicated) ||
                        result.Errors.Contains(IdentityError.InvalidEmailFormat) ||
                        result.Errors.Contains(IdentityError.MissingEmail))
                {
                    _logger.LogWarning("User email address is no longer valid.");
                    if (user is not null)
                        return RedirectToPage("./UpdateEmail", new { userId = user.Id });
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
