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
public class ForgotPasswordModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        IMediator mediator,
        ILogger<ForgotPasswordModel> logger)
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
        public Guid RequestId { get; set; }
    }

    public void OnGet()
    {
        Input.RequestId = Guid.NewGuid();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var command = new IdentifiedCommand<RequestPasswordResetCommand>(Input.RequestId, new RequestPasswordResetCommand(Input.Email));
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
        }

        // Don't reveal that the user does not exist or is not confirmed
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
