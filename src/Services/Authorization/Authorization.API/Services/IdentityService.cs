using System.Security.Claims;
using Blog.Services.Auth.API.Models;
using Blog.Services.Authorization.API.Models;

namespace Blog.Services.Auth.API.Services;

public class IdentityService : IIdentityService
{
    public ClaimsIdentity CreateClaimsIdentityFromUserIdentity(UserIdentity userIdentity, string authenticationType)
    {
        return new ClaimsIdentity(new[] {
            new Claim(Constants.UserClaims.Id, userIdentity.UserId.ToString()),
            new Claim(Constants.UserClaims.Name, userIdentity.UserName),
            new Claim(Constants.UserClaims.Email, userIdentity.Email),
            new Claim(Constants.UserClaims.Role, userIdentity.UserRole),
            new Claim(Constants.UserClaims.IsPersistent, userIdentity.IsPersistent.ToString())
        }, authenticationType);
    }

    public bool TryCreateUserIdentityFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal, out UserIdentity userIdentity)
    {
        userIdentity = null;

        try
        {
            if (!claimsPrincipal.Identity.IsAuthenticated)
                return false;

            userIdentity = new UserIdentity(
                    Guid.Parse(claimsPrincipal.FindFirstValue(Constants.UserClaims.Id)),
                    claimsPrincipal.FindFirstValue(Constants.UserClaims.Email),
                    claimsPrincipal.FindFirstValue(Constants.UserClaims.Name),
                    claimsPrincipal.FindFirstValue(Constants.UserClaims.Role),
                    bool.Parse(claimsPrincipal.FindFirstValue(Constants.UserClaims.IsPersistent)));
        }
        catch
        {
            return false;
        }

        return true;
    }
}
