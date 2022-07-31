using NodaTime;

namespace Blog.Services.Identity.API.Services;

public interface IEmailingService
{
    public Task<bool> SendEmailConfirmationEmailAsync(string username, string emailAddress, string callbackUrl, Instant urlExpirationAt);
    public Task<bool> SendPasswordResetEmailAsync(string username, string emailAddress, string callbackUrl, Instant urlExpirationAt);
}