using Blog.Services.Emailing.API.Events;
using MassTransit;

namespace Blog.Services.Emailing.API.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        throw new NotImplementedException();
    }
}
