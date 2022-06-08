using System.Security.Claims;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser> where TUser : User
{
    public async ValueTask<ClaimsPrincipal> CreateAsync(TUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        return new ClaimsPrincipal(await GenerateClaimsAsync(user).ConfigureAwait(false));
    }

    public ValueTask<ClaimsIdentity> GenerateClaimsAsync(TUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var identity = new ClaimsIdentity(
            IdentityConstants.AuthenticationScheme,
            IdentityConstants.ClaimTypes.Username,
            IdentityConstants.ClaimTypes.Role);

        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.Id, user.Id.ToString()));
        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.Username, user.Username.ToString()));
        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.Email, user.EmailAddress.ToString()));
        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString()));
        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.Role, user.Role.ToString()));
        identity.AddClaim(new Claim(IdentityConstants.ClaimTypes.SecurityStamp, user.SecurityStamp.ToString()));

        return ValueTask.FromResult(identity);
    }
}
