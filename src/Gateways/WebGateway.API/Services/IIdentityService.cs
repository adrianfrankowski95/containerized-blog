using System.Security.Claims;

namespace Blog.Gateways.WebGateway.API.Models;

public interface IIdentityService
{
    public bool TryCreateUserIdentityFromAccessToken(string accessToken, out UserIdentity userIdentity);
    public bool TryCreateUserIdentityFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal, out UserIdentity userIdentity);
}
