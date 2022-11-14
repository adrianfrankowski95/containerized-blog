#nullable disable

using Blog.Services.Identity.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Server.AspNetCore;

namespace Blog.Services.Identity.API.Pages.Connect;

[ValidateAntiForgeryToken]
public class LogoutModel : PageModel
{
    private readonly IIdentityService _identityService;

    public LogoutModel(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _identityService.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            });
    }
}
