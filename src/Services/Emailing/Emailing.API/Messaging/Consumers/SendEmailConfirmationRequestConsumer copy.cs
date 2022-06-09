using Blog.Services.Messaging.Requests;
using MassTransit;

namespace Blog.Services.Emailing.API.Messaging.Consumers;

public class SendPasswordResetRequestConsumer : IConsumer<SendPasswordResetRequest>
{
    public Task Consume(ConsumeContext<SendPasswordResetRequest> context)
    {
        throw new NotImplementedException();
    }
}
