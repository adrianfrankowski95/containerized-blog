#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Infrastructure.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

[Authorize]
public class EmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ISignInManager<User> _signInManager;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IEmailingService _emailingService;
    private readonly ISysTime _sysTime;
    private readonly ILogger<EmailModel> _logger;

    public EmailModel(
        UserManager<User> userManager,
        ISignInManager<User> signInManager,
        IOptionsMonitor<EmailOptions> emailOptions,
        IEmailingService emailingService,
        ISysTime sysTime,
        ILogger<EmailModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailOptions = emailOptions;
        _emailingService = emailingService;
        _sysTime = sysTime;
        _logger = logger;
    }

    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public InputModel Input { get; set; }


    public class InputModel
    {
        [Display(Name = "Current email")]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        [Unlike("Email", ErrorMessage = "The {0} and {1} must be different.")]
        public string NewEmail { get; set; }
    }

    public IActionResult OnGetAsync()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            LoadInput(user);
            return Page();
        }

        if (!string.Equals(Input.NewEmail, user.EmailAddress, StringComparison.OrdinalIgnoreCase))
        {
            var result = await _userManager.UpdateEmailAsync(user, Input.NewEmail);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(x => x is EmailValidationError))
                {
                    foreach (var error in result.Errors.Where(x => x is EmailValidationError))
                    {
                        ModelState.AddModelError(string.Empty, error.ErrorDescription);
                    }
                    return Page();
                }
                else if (result.Errors.Any(x => x is UsernameValidationError))
                {
                    _logger.LogWarning("User username does not meet validation requirements anymore.");
                    SaveEmail(user);
                    return RedirectToPage("./UpdateUsername", new { returnUrl = Url.Page("./Email") });
                }

                ModelState.AddModelError(string.Empty, "Invalid email change attempt.");
                return Page();
            }

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, email = Input.NewEmail, code },
                protocol: Request.Scheme);

            var isSuccess = await _emailingService.SendEmailConfirmationEmailAsync(
                    user.FullName,
                    user.EmailAddress,
                    callbackUrl,
                    _sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod)));

            if (!isSuccess)
            {
                StatusMessage = "Error sending an email. Please try again later.";
                return RedirectToPage();
            }

            StatusMessage = "Confirmation link to change email sent. Please check your email.";

            if (!_emailOptions.CurrentValue.RequireConfirmed)
                await _signInManager.RefreshSignInAsync(user);

            _logger.LogInformation("User changed their email successfully.");
            StatusMessage = "Your email has been changed.";
            return RedirectToPage();
        }

        ModelState.AddModelError(null, "The New email and Current email must be different.");
        return Page();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            LoadInput(user);
            return Page();
        }

        var result = await _userManager.GenerateEmailConfirmationAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email verification send attempt.");
            return Page();
        }

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.EmailConfirmationCode.ToString()));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { userId = user.Id, code },
            protocol: Request.Scheme);

        var isSucess = await _emailingService.SendEmailConfirmationEmailAsync(
                    user.FullName,
                    user.EmailAddress,
                    callbackUrl,
                    _sysTime.Now.Plus(Duration.FromTimeSpan(_emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod)));

        if (!isSucess)
        {
            StatusMessage = "Error sending an email. Please try again later.";
            return RedirectToPage();
        }

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }
}
