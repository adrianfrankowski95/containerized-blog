using System.Security.Claims;
using Blog.Services.Auth.API.Models;

namespace Blog.Services.Auth.API.Services;

public interface IIdentityService
{
    public ClaimsIdentity CreateClaimsIdentityFromUserIdentity(UserIdentity userIdentity, string authenticationType);
    public bool TryCreateUserIdentityFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal, out UserIdentity userIdentity);
}
