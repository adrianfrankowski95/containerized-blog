using System.Security.Claims;
using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

namespace Blog.Services.Blogging.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    public bool TryGetAuthenticatedUser(out User user)
    {
        user = null;
        try
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext.User;

            if (!claimsPrincipal.Identity.IsAuthenticated)
                return false;

            user = MapUser(claimsPrincipal);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static User MapUser(ClaimsPrincipal claimsPrincipal)
    {
        return new User(
            new UserId(Guid.Parse(claimsPrincipal.FindFirstValue(UserClaimTypes.Id))),
            claimsPrincipal.FindFirstValue(UserClaimTypes.Name),
            claimsPrincipal.FindFirstValue(UserClaimTypes.Role));
    }
}
