using Blog.Services.Messaging.Requests;
using MassTransit;

namespace Blog.Services.Emailing.API.Messaging.Consumers;

public class SendEmailConfirmationRequestConsumer : IConsumer<SendEmailConfirmationRequest>
{
    public Task Consume(ConsumeContext<SendEmailConfirmationRequest> context)
    {
        throw new NotImplementedException();
    }
}
