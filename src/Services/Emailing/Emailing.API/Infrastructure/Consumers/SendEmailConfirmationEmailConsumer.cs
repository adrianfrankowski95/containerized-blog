using Blog.Services.Messaging.Requests;
using MassTransit;

namespace Blog.Services.Emailing.API.Messaging.Consumers;

public class SendEmailConfirmationEmailConsumer : IConsumer<SendEmailConfirmationEmailRequest>
{
    public Task Consume(ConsumeContext<SendEmailConfirmationEmailRequest> context)
    {
        throw new NotImplementedException();
    }
}
