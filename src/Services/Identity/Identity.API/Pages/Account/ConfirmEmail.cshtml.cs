#nullable disable

using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConfirmEmailModel> _logger;

    public ConfirmEmailModel(IMediator mediator, ILogger<ConfirmEmailModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(ConfirmEmailAddressCommand command)
    {
        _logger.LogSendingCommand(command);

        try
        {
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            if (ex is EmailConfirmationCodeExpiredException)
                return RedirectToPage("./EmailConfirmationExpiration");

            ModelState.AddModelError(string.Empty, ex.Message);
            StatusMessage = "Error confirming your email.";
        }

        StatusMessage = "Thank you for confirming your email.";

        return Page();
    }
}
