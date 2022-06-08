using Blog.Services.Emailing.API.Events;
using MassTransit;
using NodaTime;

namespace Blog.Services.Emailing.API.Consumers;

public class EmailConfirmationRequestedEventConsumer : IConsumer<EmailConfirmationRequestedEvent>
{
    public Task Consume(ConsumeContext<EmailConfirmationRequestedEvent> context)
    {
        throw new NotImplementedException();
    }
}
