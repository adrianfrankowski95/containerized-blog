//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit

using Blog.Services.Integration.Events;
using MassTransit;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Integration.Consumers;

public class ServiceInstanceUnregisteredEventConsumer : IConsumer<ServiceInstanceUnregisteredEvent>
{
    private readonly IProxyConfigProvider _configProvider;
    private readonly ILogger<ServiceInstanceUnregisteredEventConsumer> _logger;

    public ServiceInstanceUnregisteredEventConsumer(IProxyConfigProvider configProvider, ILogger<ServiceInstanceUnregisteredEventConsumer> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<ServiceInstanceUnregisteredEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        IEnumerable<string> addresses = context.Message.ServiceAddresses;

        string addressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance unregistered event: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);

        var oldConfig = _configProvider.GetConfig();

        var oldCluster = oldConfig.Clusters.Where(cluster =>
            string.Equals(cluster.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (oldCluster is not null)
        {
            var newCluster = new ClusterConfig
            {
                ClusterId = oldCluster.ClusterId,
                Destinations = oldCluster.Destinations.Except()
            };
        }
    }
}