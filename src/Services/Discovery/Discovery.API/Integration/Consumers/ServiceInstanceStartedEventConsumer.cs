using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStartedEventConsumer : IConsumer<ServiceInstanceStartedEvent>
{
    public Task Consume(ConsumeContext<ServiceInstanceStartedEvent> context)
    {
        throw new NotImplementedException();
    }
}