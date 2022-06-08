using Blog.Services.Emailing.API.Events;
using MassTransit;

namespace Blog.Services.Emailing.API.Consumers;

public class UserEmailConfirmationRequestedEventConsumer : IConsumer<UserEmailConfirmationRequestedEvent>
{
    public Task Consume(ConsumeContext<UserEmailConfirmationRequestedEvent> context)
    {
        throw new NotImplementedException();
    }
}
