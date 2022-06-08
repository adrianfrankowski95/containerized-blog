using NodaTime;

namespace Blog.Services.Emailing.API.Events;

public record UserEmailUpdatedEvent(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);
