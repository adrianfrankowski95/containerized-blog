using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Models;
using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStartedEventConsumer : IConsumer<ServiceInstanceStartedEvent>
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceInstanceStartedEventConsumer> _logger;

    public ServiceInstanceStartedEventConsumer(
        IServiceRegistry serviceRegistry,
        ILogger<ServiceInstanceStartedEventConsumer> logger)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<ServiceInstanceStartedEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        IEnumerable<string> urls = context.Message.Urls;

        if (instanceId.Equals(Guid.Empty))
            throw new ArgumentNullException(nameof(context.Message.InstanceId));

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(context.Message.ServiceType));

        if (urls is null || !urls.Any())
            throw new ArgumentNullException(nameof(context.Message.Urls));

        string urlsString = string.Join("; ", urls);

        _logger.LogInformation("----- Handling {ServiceType} instance started event: {InstanceId} - {Urls}", serviceType, instanceId, urlsString);

        bool success = await _serviceRegistry.RegisterServiceInstance(new ServiceInfo(instanceId, serviceType, urls)).ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully registered {ServiceType} instance: {InstanceId} - {Urls}", serviceType, instanceId, urlsString);
            await context.Publish(new ServiceInstanceRegisteredEvent(instanceId, serviceType, urls)).ConfigureAwait(false);
        }

        _logger.LogError("----- Error registering {ServiceType} instance: {InstanceId} - {Urls}", serviceType, instanceId, urlsString);
    }
}