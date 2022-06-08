using Blog.Services.Emailing.API.Events;
using MassTransit;

namespace Blog.Services.Emailing.API.Consumers;

public class UserEmailUpdatedEventConsumer : IConsumer<UserEmailUpdatedEvent>
{
    public Task Consume(ConsumeContext<UserEmailUpdatedEvent> context)
    {
        throw new NotImplementedException();
    }
}
