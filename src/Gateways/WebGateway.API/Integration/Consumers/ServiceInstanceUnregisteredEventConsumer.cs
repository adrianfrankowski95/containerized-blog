//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit

using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Services;
using Blog.Services.Integration.Events;
using MassTransit;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Integration.Consumers;

public class ServiceInstanceUnregisteredEventConsumer : IConsumer<ServiceInstanceUnregisteredEvent>
{
    private readonly IInMemoryProxyConfigProvider _configProvider;
    private readonly ILogger<ServiceInstanceUnregisteredEventConsumer> _logger;

    public ServiceInstanceUnregisteredEventConsumer(IInMemoryProxyConfigProvider configProvider, ILogger<ServiceInstanceUnregisteredEventConsumer> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<ServiceInstanceUnregisteredEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;

        _logger.LogInformation("----- Handling {ServiceType} instance unregistered event: {InstanceId}", serviceType, instanceId);

        var config = _configProvider.GetConfig();

        var oldCluster = config.Clusters.Where(cluster =>
            string.Equals(cluster.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var newRoutes = config.Routes.ToList();
        var newClusters = config.Clusters.ToList();

        if (oldCluster is not null)
        {
            if (oldCluster.Destinations is not null && oldCluster.Destinations.Count > 0)
            {
                var destinationsToRemove =
                    oldCluster.Destinations.Where(k => k.Key.Contains(serviceType + "-" + instanceId.ToString()))
                    ?? Enumerable.Empty<KeyValuePair<string, DestinationConfig>>();

                var newDestinations = oldCluster.Destinations.Except(destinationsToRemove);

                if (newDestinations is not null && newDestinations.Any())
                {
                    _configProvider.GenerateCluster(oldCluster.ClusterId, newDestinations.ToDictionary(k => k.Key, v => v.Value), ref newClusters);
                    if (!config.Routes.Any(route => string.Equals(route.ClusterId, oldCluster.ClusterId.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        var paths = PathsConfig.GetServiceMatchingPaths(serviceType);
                        _configProvider.GenerateRoutes(serviceType, paths, ref newRoutes);
                    }
                }
            }
            else
                newRoutes.RemoveAll(route => string.Equals(route.ClusterId, oldCluster.ClusterId, StringComparison.OrdinalIgnoreCase));

            newClusters.Remove(oldCluster);
            _configProvider.Update(newRoutes, newClusters);
        }

        int removedRoutes = newRoutes.RemoveAll(route => string.Equals(route.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase));
        if (removedRoutes > 0)
            _configProvider.Update(newRoutes, newClusters);

        return Task.CompletedTask;
    }
}