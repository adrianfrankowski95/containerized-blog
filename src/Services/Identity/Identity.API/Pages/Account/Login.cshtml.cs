#nullable disable


using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.API.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

[AllowAnonymous]
[ValidateAntiForgeryToken]
public class LoginModel : PageModel
{
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IIdentityService identityService, IMediator mediator, ILogger<LoginModel> logger)
    {
        _identityService = identityService;
        _mediator = mediator;
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

        [Required]
        public Guid RequestId { get; set; }
    }

    public IActionResult OnGet(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
            ModelState.AddModelError(string.Empty, ErrorMessage);

        ReturnUrl = returnUrl ?? Url.Content("~/");
        Input.RequestId = Guid.NewGuid();

        return _identityService.IsAuthenticated ? LocalRedirect(ReturnUrl) : Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
            return Page();

        var command = new IdentifiedCommand<LogInCommand>(Input.RequestId, new LogInCommand(Input.Email, Input.Password, Input.RememberMe));
        _logger.LogSendingCommand(command);

        try
        {
            await _mediator.Send(command);
            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
