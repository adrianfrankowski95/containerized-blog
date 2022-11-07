// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ISignInManager<User> _signInManager;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IEmailingService _emailingService;
    private readonly ISysTime _sysTime;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<User> userManager,
        ISignInManager<User> signInManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        IEmailingService emailingService,
        ISysTime sysTime,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailOptions = emailOptions;
        _emailingService = emailingService;
        _sysTime = sysTime;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [StringLength(50, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and Password Confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at at max {1} characters long.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at max {1} characters long.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "I would like to receive additional updates and announcements via email")]
        public bool ReceiveAdditionalEmails { get; set; }
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var user = new User(Input.Email, Input.Username, Input.FirstName, Input.LastName, Input.ReceiveAdditionalEmails);

            var result = await _userManager.CreateUserAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorDescription);
                }
                return Page();
            }

            _logger.LogInformation("User created a new account with password.");
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()!));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, code, returnUrl },
                protocol: Request.Scheme);

            var isSuccess = await _emailingService.SendEmailConfirmationEmailAsync(
                    user.FullName,
                    user.EmailAddress,
                    callbackUrl,
                    _sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod)));

            if (!isSuccess)
            {
                return RedirectToPage("./EmailConfirmationSendError");
            }

            if (_emailOptions.CurrentValue.RequireConfirmed)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
            }
            else
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
