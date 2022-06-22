using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Models;
using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Services.Discovery.API.Integration.Consumers;

public class ServiceInstanceHeartbeatEventConsumer : IConsumer<ServiceInstanceHeartbeatEvent>
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceInstanceHeartbeatEventConsumer> _logger;

    public ServiceInstanceHeartbeatEventConsumer(
        IServiceRegistry serviceRegistry,
        ILogger<ServiceInstanceHeartbeatEventConsumer> logger)
    {
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task Consume(ConsumeContext<ServiceInstanceHeartbeatEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        IEnumerable<string> serviceUrls = context.Message.ServiceUrls;

        string serviceUrlsString = string.Join("; ", serviceUrls);

        _logger.LogInformation("----- Handling {ServiceType} instance heartbeat event: {InstanceId} - {Urls}", serviceType, instanceId, serviceUrlsString);

        var serviceInfo = new ServiceInfo(instanceId, serviceType, serviceUrls);
        bool exists = await _serviceRegistry.TryRefreshServiceInstanceExpiry(serviceInfo).ConfigureAwait(false);

        if (!exists)
        {
            _logger.LogInformation("----- Registering new {ServiceType} instance recognized by a heartbeat: {InstanceId} - {Urls}",
                serviceType, instanceId, serviceUrlsString);

            bool success = await _serviceRegistry.RegisterServiceInstance(serviceInfo).ConfigureAwait(false);

            if (success)
            {
                _logger.LogInformation("----- Successfully registered {ServiceType} instance: {InstanceId} - {Urls}", serviceType, instanceId, serviceUrlsString);
                await context.Publish(new ServiceInstanceRegisteredEvent(instanceId, serviceType, serviceUrls)).ConfigureAwait(false);
            }

            _logger.LogError("----- Error registering {ServiceType} instance: {InstanceId} - {Urls}", serviceType, instanceId, serviceUrlsString);
        }
        _logger.LogWarning("----- The {ServiceType} instance already exists: {InstanceId} - {Urls}", serviceType, instanceId, serviceUrlsString);
    }
}