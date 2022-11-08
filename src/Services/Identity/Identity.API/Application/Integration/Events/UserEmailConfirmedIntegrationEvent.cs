// Namespace of the event must be the same in Producers and in Consumers to make it work through MassTransit
namespace Blog.Integration.Events;

public record UserEmailConfirmedIntegrationEvent(
    string Username,
    string EmailAddress);
