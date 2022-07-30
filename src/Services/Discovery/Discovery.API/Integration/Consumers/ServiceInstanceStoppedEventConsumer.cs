using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Models;
using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStoppedEventConsumer : IConsumer<ServiceInstanceStoppedEvent>
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceInstanceStoppedEventConsumer> _logger;

    public ServiceInstanceStoppedEventConsumer(
        IServiceRegistry serviceRegistry,
        ILogger<ServiceInstanceStoppedEventConsumer> logger)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task Consume(ConsumeContext<ServiceInstanceStoppedEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        HashSet<string> addresses = context.Message.ServiceAddresses;

        if (instanceId.Equals(Guid.Empty))
            throw new ArgumentNullException(nameof(context.Message.InstanceId));

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(context.Message.ServiceType));

        if (addresses is null || !addresses.Any())
            throw new ArgumentNullException(nameof(context.Message.ServiceAddresses));

        string addressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance stopped event: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);

        bool success = await _serviceRegistry.UnregisterServiceInstance(new ServiceInstance(instanceId, serviceType, addresses)).ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully unregistered {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);
            await context.Publish(new ServiceInstanceUnregisteredEvent(instanceId, serviceType)).ConfigureAwait(false);
        }

        _logger.LogError("----- Error unregistering {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);
    }
}