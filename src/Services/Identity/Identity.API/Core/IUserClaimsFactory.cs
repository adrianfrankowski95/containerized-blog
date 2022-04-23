using System.Security.Claims;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUserClaimsPrincipalFactory
{
    public ValueTask<ClaimsPrincipal> CreateAsync(User user);
    protected ValueTask<ClaimsIdentity> GenerateClaimsAsync(User user);
}
