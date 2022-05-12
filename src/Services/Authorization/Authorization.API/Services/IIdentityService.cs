using System.Security.Claims;
using Blog.Services.Authorization.API.Models;

namespace Blog.Services.Authorization.API.Services;

public interface IIdentityService
{
    public ClaimsIdentity CreateClaimsIdentityFromUserIdentity(UserIdentity userIdentity, string authenticationType);
    public bool TryCreateUserIdentityFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal, out UserIdentity userIdentity);
}
