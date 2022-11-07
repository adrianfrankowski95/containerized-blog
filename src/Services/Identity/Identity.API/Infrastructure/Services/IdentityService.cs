using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Authentication;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal User
    {
        get => _httpContextAccessor?.HttpContext is null
            ? throw new InvalidOperationException("Could not retrieve user data from an Http context.")
            : _httpContextAccessor.HttpContext.User;
    }

    public bool IsAuthenticated
    {
        get => User.Identity is null
                ? throw new InvalidOperationException("Could not retrieve a user identity from an Http context.")
                : User.Identity.IsAuthenticated;
    }

    public UserId? UserId
    {
        get
        {
            var id = Guid.TryParse(User.FindFirstValue(IdentityConstants.UserClaimTypes.Id), out Guid userId) ? UserId.FromGuid(userId) : null;

            return IsAuthenticated && id is null
                ? throw new InvalidOperationException("Could not retrieve an ID of an authenticated user.")
                : id;
        }
    }

    public NonEmptyString? Username
    {
        get
        {
            var username = User.FindFirstValue(IdentityConstants.UserClaimTypes.Username);

            return IsAuthenticated && string.IsNullOrWhiteSpace(username)
                ? throw new InvalidOperationException("Could not retrieve a username of an authenticated user.")
                : username;
        }
    }

    public UserRole? UserRole
    {
        get
        {
            string role = User.FindFirstValue(IdentityConstants.UserClaimTypes.Role);
            return IsAuthenticated
                ? (string.IsNullOrWhiteSpace(User.FindFirstValue(IdentityConstants.UserClaimTypes.Role))
                    ? throw new InvalidOperationException("Could not retrieve role of an authenticated user.")
                    : UserRole.FromName(role))
                : null;
        }
    }

    public bool IsInRole(UserRole role) => User.IsInRole(role.Name);

    private ClaimsIdentity CreateUserIdentity(User user)
        => new ClaimsIdentity(
                new[] {
                    new Claim(IdentityConstants.UserClaimTypes.Id, user.Id.ToString()),
                    new Claim(IdentityConstants.UserClaimTypes.Username, user.Username),
                    new Claim(IdentityConstants.UserClaimTypes.FirstName, user.FullName.FirstName),
                    new Claim(IdentityConstants.UserClaimTypes.LastName, user.FullName.LastName),
                    new Claim(IdentityConstants.UserClaimTypes.Email, user.EmailAddress),
                    new Claim(IdentityConstants.UserClaimTypes.Role, user.Role.ToString()),
                    new Claim(IdentityConstants.UserClaimTypes.SecurityStamp, user.SecurityStamp.ToString())},
                IdentityConstants.AuthenticationSchemes.IdentityService,
                IdentityConstants.UserClaimTypes.Username,
                IdentityConstants.UserClaimTypes.Role);

    public Task SignInAsync(User user, bool isPersistent)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        if (_httpContextAccessor?.HttpContext is null)
            throw new InvalidOperationException("Could not retrieve an Http context.");

        return _httpContextAccessor.HttpContext.SignInAsync(
            new ClaimsPrincipal(CreateUserIdentity(user)),
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                AllowRefresh = true
            });
    }

    public async Task RefreshSignInAsync(User user)
    {
        if (_httpContextAccessor?.HttpContext is null)
            throw new InvalidOperationException("Could not retrieve an Http context.");

        if (!IsAuthenticated)
            throw new InvalidOperationException("Cannot refresh unauthenticated user data.");

        var auth = await _httpContextAccessor.HttpContext.AuthenticateAsync().ConfigureAwait(false);

        await _httpContextAccessor.HttpContext.SignInAsync(new ClaimsPrincipal(CreateUserIdentity(user)), auth.Properties);
    }

    public Task SignOutAsync()
    {
        if (_httpContextAccessor?.HttpContext is null)
            throw new InvalidOperationException("Could not retrieve an Http context.");

        return _httpContextAccessor.HttpContext.SignOutAsync();
    }

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
}
