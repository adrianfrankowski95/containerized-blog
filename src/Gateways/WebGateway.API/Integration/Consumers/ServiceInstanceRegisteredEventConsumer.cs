//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit

using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Models;
using Blog.Gateways.WebGateway.API.Services;
using Blog.Services.Integration.Events;
using MassTransit;

namespace Blog.Gateways.WebGateway.API.Integration.Consumers;

public class ServiceInstanceRegisteredEventConsumer : IConsumer<ServiceInstanceRegisteredEvent>
{
    private readonly IInMemoryProxyConfigProvider _configProvider;
    private readonly ILogger<ServiceInstanceRegisteredEventConsumer> _logger;

    public ServiceInstanceRegisteredEventConsumer(IInMemoryProxyConfigProvider configProvider, ILogger<ServiceInstanceRegisteredEventConsumer> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<ServiceInstanceRegisteredEvent> context)
    {
        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        HashSet<string> addresses = context.Message.ServiceAddresses;

        string addressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance registered event: {InstanceId} - {Addresses}", serviceType, instanceId, addressesString);

        var config = _configProvider.GetConfig();
        var newRoutes = config.Routes.ToList();
        var newClusters = config.Clusters.ToList();

        var oldCluster = config.Clusters.Where(cluster =>
            string.Equals(cluster.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var newDestinations = _configProvider.GenerateDestinations(
            serviceType,
            new HashSet<ServiceInstance>() { new ServiceInstance(instanceId, addresses) });

        // If cluster for this service type already existed, merge existing destinations with new ones,
        // if cluster did not exist, generate new routes from scratch based on matching paths from config file
        if (oldCluster is not null)
        {
            newClusters.Remove(oldCluster);

            if (oldCluster.Destinations is not null && oldCluster.Destinations.Count > 0)
                newDestinations = newDestinations.Union(oldCluster.Destinations).ToDictionary(k => k.Key, v => v.Value);
        }
        else
        {
            var paths = PathsConfig.GetMatchingPaths(serviceType);
            _configProvider.GenerateRoutes(serviceType, paths, ref newRoutes);
        }

        _configProvider.GenerateCluster(serviceType, newDestinations, ref newClusters);
        _configProvider.Update(newRoutes, newClusters);

        return Task.CompletedTask;

    }
}