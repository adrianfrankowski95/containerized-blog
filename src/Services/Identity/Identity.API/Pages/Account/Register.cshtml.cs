#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ISysTime _sysTime;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        IMediator mediator,
        ISysTime sysTime,
        ILogger<RegisterModel> logger)
    {
        _mediator = mediator;
        _sysTime = sysTime;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

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

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "I would like to receive additional updates and announcements via email")]
        public bool ReceiveAdditionalEmails { get; set; }

        [Required]
        public Guid RequestId { get; set; }
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        Input.RequestId = Guid.NewGuid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
            return Page();

        var command = new IdentifiedCommand<RegisterCommand>(
                Input.RequestId,
                new RegisterCommand(
                    Input.Username,
                    Input.FirstName,
                    Input.Gender,
                    Input.LastName,
                    Input.ReceiveAdditionalEmails,
                    Input.Email,
                    Input.Password));

        _logger.LogSendingCommand(command);

        try
        {
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error requesting a password reset, command: @Command", command);

            if (ex is EmailingServiceException)
            {
                StatusMessage = "Error sending an email. Please try again later.";
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, ex.Message);
            StatusMessage = "Error registering your account.";
            return Page();
        }

        StatusMessage = "Account registered successfully.";
        return LocalRedirect(returnUrl);
    }
}
