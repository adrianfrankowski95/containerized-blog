using System.ComponentModel;
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
        string serviceType = context.Message.ServiceType;
        IEnumerable<string> serviceUrls = context.Message.serviceUrls;

        _logger.LogInformation("----- Handling service started event: {ServiceType} - {Urls}", serviceType, serviceUrls);

        if (!Enum.TryParse(serviceType, true, out ServiceType serviceTypeEnum))
        {
            _logger.LogCritical("----- Error registering service instance - unrecognized service type: {ServiceType}", serviceType);
            throw new InvalidEnumArgumentException();
        }

        (long changes, ServiceInfo updatedServiceInfo) = await _serviceRegistry
            .RegisterService(new ServiceInfo(serviceTypeEnum, serviceUrls))
            .ConfigureAwait(false);

        if (changes > 0)
        {
            _logger.LogInformation("----- Successfully registered {UrlsCount} URL(s) of {ServiceType}", changes, serviceType);

            await context.Publish(new ServiceRegistryUpdatedEvent(
                updatedServiceInfo.Type.ToString(), updatedServiceInfo.Urls))
                .ConfigureAwait(false);
        }

        _logger.LogWarning("----- No new URLs have been registered for {ServiceType}", serviceType);
    }
}