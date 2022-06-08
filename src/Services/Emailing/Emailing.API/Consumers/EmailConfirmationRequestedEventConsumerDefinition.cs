using Blog.Services.Emailing.API.Events;
using MassTransit;
using NodaTime;

namespace Blog.Services.Emailing.API.Consumers;

public class EmailConfirmationRequestedEventConsumerDefinition : ConsumerDefinition<EmailConfirmationRequestedEventConsumer>
{
    public EmailConfirmationRequestedEventConsumerDefinition()
    {
        EndpointName = "emailing-api";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<EmailConfirmationRequestedEventConsumer> consumerConfigurator)
    {
        // configure message retry with millisecond intervals
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

        // use the outbox to prevent duplicate events from being published
        endpointConfigurator.UseInMemoryOutbox();
    }
}
