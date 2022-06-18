using System.ComponentModel;
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
        string serviceType = context.Message.ServiceType;
        IEnumerable<string> serviceUrls = context.Message.ServiceBaseUrls;

        _logger.LogInformation("----- Handling service stopped event: {ServiceType} - {Urls}", serviceType, serviceUrls);

        if (!Enum.TryParse(serviceType, true, out ServiceType serviceTypeEnum))
        {
            _logger.LogCritical("----- Error removing service URLs - unrecognized service type: {ServiceType}", serviceType);
            throw new InvalidEnumArgumentException();
        }

        var result = await _serviceRegistry
            .UnregisterService(new ServiceInfo(serviceTypeEnum, serviceUrls))
            .ConfigureAwait(false);

        _logger.LogInformation("----- Successfully removed {UrlsCount} URL(s) of {ServiceType}", result, serviceType);
    }
}