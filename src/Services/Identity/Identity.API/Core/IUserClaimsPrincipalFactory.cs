using System.Security.Claims;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUserClaimsPrincipalFactory<TUser> where TUser : User
{
    public ValueTask<ClaimsPrincipal> CreateAsync(TUser user);
    protected ValueTask<ClaimsIdentity> GenerateClaimsAsync(TUser user);
}
