using System.Security.Claims;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication;

namespace Blog.Services.Identity.API.Adapters;

public class SignInManager : ISignInManager<User>
{
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SignInManager(
        IHttpContextAccessor httpContextAccessor,
        IUserClaimsPrincipalFactory<User> userClaimsFactory)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _userClaimsFactory = userClaimsFactory ?? throw new ArgumentNullException(nameof(userClaimsFactory));
    }

    public async Task SignInAsync(User user, bool isPersistent)
    {
        var principal = await _userClaimsFactory.CreateAsync(user).ConfigureAwait(false);

        await _httpContextAccessor.HttpContext!.SignInAsync(
            IdentityConstants.AuthenticationScheme,
            principal,
            new AuthenticationProperties() { IsPersistent = isPersistent });
    }

    public async Task<bool> RefreshSignInAsync(User user)
    {
        var auth = await _httpContextAccessor.HttpContext!.AuthenticateAsync(IdentityConstants.AuthenticationScheme).ConfigureAwait(false);

        if (!auth.Succeeded || !IsSignedIn(auth.Principal))
            return false;

        await SignInAsync(user, auth.Properties.IsPersistent).ConfigureAwait(false);
        return true;
    }

    public Task SignOutAsync()
     => _httpContextAccessor.HttpContext!.SignOutAsync(IdentityConstants.AuthenticationScheme);


    public bool IsSignedIn(ClaimsPrincipal principal)
        => principal.Identities is not null &&
            principal.Identities.Any(x => x.AuthenticationType is IdentityConstants.AuthenticationScheme);

    public bool VerifySecurityStamp(User? user, ClaimsPrincipal? principal)
    {
        if (user is null || principal is null)
            return false;

        if (!Guid.TryParse(principal.FindFirstValue(IdentityConstants.ClaimTypes.SecurityStamp), out Guid securityStamp))
            return false;

        return user.SecurityStamp.Equals(securityStamp);
    }
}
