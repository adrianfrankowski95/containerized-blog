using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public interface IIdentityService
{
    public bool IsAuthenticated { get; }
    public UserId? UserId { get; }
    public string? Username { get; }
    public UserRole? UserRole { get; }
    public bool IsInRole(UserRole role);
    public Task SignInAsync(User user, bool isPersistent);
    public Task RefreshSignInAsync(User user);
    public Task SignOutAsync();
}
