using NodaTime;

namespace Blog.Services.Emailing.API.Events;

public record UserEmailConfirmationRequestedEvent(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);
