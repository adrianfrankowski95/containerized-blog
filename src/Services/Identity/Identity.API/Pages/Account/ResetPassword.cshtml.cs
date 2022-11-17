#nullable disable

using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(
        IMediator mediator,
        ILogger<ResetPasswordModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = Password.MinLength)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and Password Confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public Guid RequestId { get; set; }
    }

    public IActionResult OnGet(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        Input.RequestId = Guid.NewGuid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var command = new IdentifiedCommand<ResetPasswordCommand>(
            Input.RequestId,
            new ResetPasswordCommand(Input.Email, Input.NewPassword, Input.Code));
        _logger.LogSendingCommand(command);

        try
        {
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error resetting a password, command: @Command", command);

            if (ex is PasswordResetCodeExpirationException)
                return RedirectToPage("./ResetPasswordExpiration");

            ModelState.AddModelError(string.Empty, ex.Message);
            StatusMessage = "Error resetting your password.";
        }

        return RedirectToPage("./ResetPasswordConfirmation");
    }
}
