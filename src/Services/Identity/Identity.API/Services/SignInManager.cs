using System.Security.Claims;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication;
using NodaTime;

namespace Blog.Services.Identity.API.Services;

public class SignInManager<TUser> : ISignInManager<TUser> where TUser : User
{
    private readonly IUserClaimsPrincipalFactory<TUser> _userClaimsFactory;

    public SignInManager(IUserClaimsPrincipalFactory<TUser> userClaimsFactory)
    {
        _userClaimsFactory = userClaimsFactory ?? throw new ArgumentNullException(nameof(userClaimsFactory));
    }

    public async Task SignInAsync(HttpContext context, TUser user, bool isPersistent)
    {
        var principal = await _userClaimsFactory.CreateAsync(user).ConfigureAwait(false);

        await context.SignInAsync(
            IdentityConstants.AuthenticationScheme,
            principal,
            new AuthenticationProperties() { IsPersistent = isPersistent });
    }

    public async Task SignOutAsync(HttpContext context)
    {
        await context.SignOutAsync(IdentityConstants.AuthenticationScheme);
    }

    public bool IsSignedIn(ClaimsPrincipal principal)
        => principal.Identities is not null &&
            principal.Identities.Any(x => x.AuthenticationType is IdentityConstants.AuthenticationScheme);
}
