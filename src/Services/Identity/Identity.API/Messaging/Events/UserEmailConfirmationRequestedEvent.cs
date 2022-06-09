using NodaTime;

//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit
namespace Blog.Services.Messaging.Events;

public record UserEmailConfirmationRequestedEvent(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);
