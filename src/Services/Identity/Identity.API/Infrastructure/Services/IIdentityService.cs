using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public interface IIdentityService
{
    public bool IsAuthenticated { get; }
    public UserId? UserId { get; }
    public UserRole? UserRole { get; }
    public bool IsInRole(UserRole role);
}
