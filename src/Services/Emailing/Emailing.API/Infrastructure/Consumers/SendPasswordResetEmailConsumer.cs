using Blog.Services.Messaging.Requests;
using MassTransit;

namespace Blog.Services.Emailing.API.Messaging.Consumers;

public class SendPasswordResetEmailConsumer : IConsumer<SendPasswordResetEmailRequest>
{
    public Task Consume(ConsumeContext<SendPasswordResetEmailRequest> context)
    {
        throw new NotImplementedException();
    }
}
