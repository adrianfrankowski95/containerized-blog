using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal User
    {
        get
        {
            if (_httpContextAccessor is null || _httpContextAccessor.HttpContext is null)
                throw new InvalidOperationException("Could not retreive user data from an Http context.");

            return _httpContextAccessor.HttpContext.User;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            if (User.Identity is null)
                throw new InvalidOperationException("Could not retreive a user identity from an Http context.");

            return User.Identity.IsAuthenticated;
        }
    }

    public UserId? UserId
    {
        get
        {
            var id = Guid.TryParse(User.FindFirstValue(UserClaimTypes.Id), out Guid userId) ? UserId.FromGuid(userId) : null;

            if (IsAuthenticated && id is null)
                throw new InvalidOperationException("Could not retreive an ID of an authenticated user.");

            return id;
        }
    }

    public NonEmptyString? Username
    {
        get
        {
            var username = User.FindFirstValue(UserClaimTypes.Name);

            if (IsAuthenticated && string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Could not retreive an ID of an authenticated user.");

            return username;
        }
    }

    public UserRole? UserRole
    {
        get
        {
            string role = User.FindFirstValue(UserClaimTypes.Role);
            return IsAuthenticated
                ? (string.IsNullOrWhiteSpace(User.FindFirstValue(UserClaimTypes.Role))
                    ? throw new InvalidOperationException("Could not retreive role of an authenticated user.")
                    : UserRole.FromName(role))
                : null;
        }
    }

    public bool IsInRole(UserRole role) => User.IsInRole(role.Name);

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }


}
