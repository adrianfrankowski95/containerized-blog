using System.Security.Claims;

namespace Blog.Services.Identity.API.Core;

public interface ISecurityStampValidator
{
    public ValueTask<IdentityResult> ValidateAsync(ClaimsPrincipal principal);
}
