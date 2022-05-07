using System.Security.Claims;
using Blog.Services.Authorization.API.Models;

namespace Blog.Services.Auth.API.Services;

public class IdentityService : IIdentityService
{
    public ClaimsIdentity CreateClaimsIdentityFromUserIdentity(UserIdentity userIdentity, string authenticationType)
    {
        return new ClaimsIdentity(new[] {
            new Claim(Constants.UserClaimTypes.Id, userIdentity.UserId.ToString()),
            new Claim(Constants.UserClaimTypes.Name, userIdentity.UserName),
            new Claim(Constants.UserClaimTypes.Email, userIdentity.Email),
            new Claim(Constants.UserClaimTypes.Role, userIdentity.UserRole),
            new Claim(Constants.UserClaimTypes.IsPersistent, userIdentity.IsPersistent.ToString())
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
                    Guid.Parse(claimsPrincipal.FindFirstValue(Constants.UserClaimTypes.Id)),
                    claimsPrincipal.FindFirstValue(Constants.UserClaimTypes.Email),
                    claimsPrincipal.FindFirstValue(Constants.UserClaimTypes.Name),
                    claimsPrincipal.FindFirstValue(Constants.UserClaimTypes.Role),
                    bool.Parse(claimsPrincipal.FindFirstValue(Constants.UserClaimTypes.IsPersistent)));
        }
        catch
        {
            return false;
        }

        return true;
    }
}
