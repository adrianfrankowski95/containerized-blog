using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal? User => _httpContextAccessor?.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public UserId? Id => Guid.TryParse(User?.FindFirstValue(UserClaimTypes.Id), out Guid userId) ? UserId.FromGuid(userId) : null;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public bool IsInRole(UserRole role)
    {
        if (!IsAuthenticated)
            return false;

        try
        {
            var userRole = UserRole.FromName(User?.FindFirstValue(UserClaimTypes.Role) ?? "");
            return userRole.Equals(role);
        }
        catch
        {
            return false;
        }
    }
}
