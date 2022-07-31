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
        HashSet<string> addresses = context.Message.ServiceAddresses;

        if (instanceId.Equals(Guid.Empty))
            throw new InvalidDataException($"{nameof(context.Message.InstanceId)} must not be empty");

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new InvalidDataException($"{nameof(context.Message.ServiceType)} must not be null or empty");

        if (addresses is null || !addresses.Any())
            throw new InvalidDataException($"{nameof(context.Message.ServiceAddresses)} must not be null or empty");

        string AddressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance started event: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);

        bool success = await _serviceRegistry.RegisterServiceInstance(new ServiceInstance(instanceId, serviceType, addresses)).ConfigureAwait(false);

        if (success)
        {
            _logger.LogInformation("----- Successfully registered {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);
            await context.Publish(new ServiceInstanceRegisteredEvent(instanceId, serviceType, addresses)).ConfigureAwait(false);
        }

        _logger.LogError("----- Error registering {ServiceType} instance: {InstanceId} - {Addresses}", serviceType, instanceId, AddressesString);
    }
}