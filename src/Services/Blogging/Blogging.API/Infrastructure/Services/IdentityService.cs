using System.Security.Claims;
using Blog.Services.Blogging.API.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    public User GetCurrentUser()
    {
        var claimsPrincipal = _httpContextAccessor?.HttpContext?.User;

        bool? isAuthenticated = claimsPrincipal?.Identity?.IsAuthenticated;

        if (claimsPrincipal is null || isAuthenticated is null || !isAuthenticated.Value)
            throw new BloggingDomainException("Error authenticating the user");

        return MapUser(claimsPrincipal);
    }

    private static User MapUser(ClaimsPrincipal claimsPrincipal)
    {
        return new User(
            new UserId(Guid.Parse(claimsPrincipal.FindFirstValue(UserClaimTypes.Id))),
            claimsPrincipal.FindFirstValue(UserClaimTypes.Name),
            claimsPrincipal.FindFirstValue(UserClaimTypes.Role));
    }
}
