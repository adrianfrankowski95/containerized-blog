#nullable disable

using Blog.Services.Identity.API.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blog.Services.Identity.API.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(IIdentityService identityService, ILogger<LogoutModel> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _identityService.SignOutAsync();
        _logger.LogInformation("User logged out.");

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new
            // request and the identity for the user gets updated.
            return RedirectToPage();
        }
    }
}
