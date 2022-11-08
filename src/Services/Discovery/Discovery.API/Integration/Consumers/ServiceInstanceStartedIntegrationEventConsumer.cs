using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Models;
using Blog.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceStartedIntegrationEventConsumer : IConsumer<ServiceInstanceStartedIntegrationEvent>
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceInstanceStartedIntegrationEventConsumer> _logger;

    public ServiceInstanceStartedIntegrationEventConsumer(
        IServiceRegistry serviceRegistry,
        ILogger<ServiceInstanceStartedIntegrationEventConsumer> logger)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<ServiceInstanceStartedIntegrationEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        IReadOnlySet<string> addresses = context.Message.ServiceAddresses;

        if (instanceId.Equals(Guid.Empty))
            throw new InvalidDataException($"{nameof(context.Message.InstanceId)} must not be empty");

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new InvalidDataException($"{nameof(context.Message.ServiceType)} must not be null or empty");

        if (!(addresses?.Any() ?? false))
            throw new InvalidDataException($"{nameof(context.Message.ServiceAddresses)} must not be null or empty");

        string AddressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance started event: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);

        bool success = await _serviceRegistry.RegisterServiceInstance(new ServiceInstance(instanceId, serviceType, addresses)).ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully registered {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);
            await context.Publish(new ServiceInstanceRegisteredIntegrationEvent(instanceId, serviceType, addresses)).ConfigureAwait(false);
        }
        else
            _logger.LogError("----- Error registering {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);
    }
}