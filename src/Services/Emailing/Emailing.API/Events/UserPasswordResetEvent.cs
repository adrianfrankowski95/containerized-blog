using NodaTime;

namespace Blog.Services.Emailing.API.Events;

public record UserPasswordResetEvent(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);
