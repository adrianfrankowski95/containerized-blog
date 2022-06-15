using NodaTime;

//namespace of the request must be the same in Producers and in Consumers
//in order to make it work through the MassTransit
namespace Blog.Services.Messaging.Requests;

public record SendEmailConfirmationEmailRequest(
    string Username,
    string EmailAddress,
    string CallbackUrl,
    Instant UrlValidUntil);

