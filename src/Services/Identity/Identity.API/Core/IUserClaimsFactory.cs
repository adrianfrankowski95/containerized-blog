using System.Security.Claims;

namespace Blog.Services.Identity.API.Core;

public interface IUserClaimsPrincipalFactory
{
    public ValueTask<ClaimsPrincipal> CreateAsync(User user);
    protected ValueTask<ClaimsIdentity> GenerateClaimsAsync(User user);
}
