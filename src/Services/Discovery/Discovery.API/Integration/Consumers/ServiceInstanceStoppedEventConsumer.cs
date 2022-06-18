using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStoppedEventConsumer : IConsumer<ServiceInstanceStoppedEvent>
{
    public Task Consume(ConsumeContext<ServiceInstanceStoppedEvent> context)
    {
        throw new NotImplementedException();
    }
}