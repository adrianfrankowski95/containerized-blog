using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Models;
using Blog.Integration.Events;
using MassTransit;
using Blog.Services.Discovery.API.Extensions;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStoppedIntegrationEventConsumer : IConsumer<ServiceInstanceStoppedIntegrationEvent>
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceInstanceStoppedIntegrationEventConsumer> _logger;

    public ServiceInstanceStoppedIntegrationEventConsumer(
        IServiceRegistry serviceRegistry,
        ILogger<ServiceInstanceStoppedIntegrationEventConsumer> logger)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task Consume(ConsumeContext<ServiceInstanceStoppedIntegrationEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        IReadOnlySet<string> addresses = context.Message.ServiceAddresses;

        if (instanceId.Equals(Guid.Empty))
            throw new InvalidDataException($"{nameof(context.Message.InstanceId)} must not be empty");

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new InvalidDataException($"{nameof(context.Message.ServiceType)} must not be null or empty");

        if (addresses.IsNullOrEmpty())
            throw new InvalidDataException($"{nameof(context.Message.ServiceAddresses)} must not be null or empty");

        string addressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance stopped event: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);

        bool success = await _serviceRegistry.UnregisterServiceInstance(new ServiceInstance(instanceId, serviceType, addresses)).ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully unregistered {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);
            await context.Publish(new ServiceInstanceUnregisteredIntegrationEvent(instanceId, serviceType)).ConfigureAwait(false);
        }
        else
            _logger.LogError("----- Error unregistering {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);
    }
}