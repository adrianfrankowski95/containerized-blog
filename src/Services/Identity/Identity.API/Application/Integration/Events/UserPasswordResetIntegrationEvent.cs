using NodaTime;

// Namespace of the event must be the same in Producers and in Consumers to make it work through MassTransit
namespace Blog.Integration.Events;

public record UserPasswordResetIntegrationEvent(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);
