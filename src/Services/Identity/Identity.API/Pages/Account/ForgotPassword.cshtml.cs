#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NodaTime;

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
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var command = new ResetPasswordCommand(Input.Email);
        _logger.LogSendingCommand(command);

        try
        {
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error confirming email address, command: @Command", command);

        }

        // Don't reveal that the user does not exist or is not confirmed
        return RedirectToPage("./ForgotPasswordConfirmation");

        var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { code },
                protocol: Request.Scheme);

        // Don't reveal any validation details
        if (result.Succeeded)
        {
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.PasswordResetCode));


            var isSuccess = await _emailingService.SendPasswordResetEmailAsync(
                user.FullName,
                user.EmailAddress,
                callbackUrl,
                _sysTime.Now.Plus(Duration.FromTimeSpan(_passwordOptions.CurrentValue.PasswordResetCodeValidityPeriod)));

            if (!isSuccess)
            {
                StatusMessage = "Error sending an email. Please try again later.";
                return RedirectToPage();
            }
        }
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
