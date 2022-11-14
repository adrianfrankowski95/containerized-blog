using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static UserId? GetUserId(this ClaimsPrincipal? principal)
        => Guid.TryParse(principal?.FindFirstValue(IdentityConstants.UserClaimTypes.Subject), out Guid userId)
            ? UserId.FromGuid(userId)
            : null;

    public static string? GetUsername(this ClaimsPrincipal? principal)
        => principal?.FindFirstValue(IdentityConstants.UserClaimTypes.Username);

    public static UserRole? GetUserRole(this ClaimsPrincipal? principal)
    {
        string? role = principal?.FindFirstValue(IdentityConstants.UserClaimTypes.Username);
        return string.IsNullOrWhiteSpace(role) ? null : UserRole.FromName(role);
    }

    public static EmailAddress? GetEmailAddress(this ClaimsPrincipal? principal)
    {
        string? email = principal?.FindFirstValue(IdentityConstants.UserClaimTypes.Email);
        return string.IsNullOrWhiteSpace(email) ? null : new EmailAddress(email);
    }

    public static string? GetFirstName(this ClaimsPrincipal? principal)
        => principal?.FindFirstValue(IdentityConstants.UserClaimTypes.FirstName);

    public static string? GetLastName(this ClaimsPrincipal? principal)
        => principal?.FindFirstValue(IdentityConstants.UserClaimTypes.LastName);

    public static Guid? GetSecurityStamp(this ClaimsPrincipal? principal)
        => Guid.TryParse(principal?.FindFirstValue(IdentityConstants.UserClaimTypes.SecurityStamp), out Guid securityStamp)
            ? securityStamp
            : null;
}