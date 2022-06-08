using Blog.Services.Emailing.API.Events;
using MassTransit;

namespace Blog.Services.Emailing.API.Consumers;

public class UserPasswordResetEventConsumer : IConsumer<UserPasswordResetEvent>
{
    public Task Consume(ConsumeContext<UserPasswordResetEvent> context)
    {
        throw new NotImplementedException();
    }
}
