//namespace of the event must be the same in Producers and in Consumers
//in order to make it work through the MassTransit

using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Models;
using Blog.Gateways.WebGateway.API.Services;
using Blog.Integration.Events;
using MassTransit;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Integration.Consumers;

public class ServiceInstanceRegisteredIntegrationEventConsumer : IConsumer<ServiceInstanceRegisteredIntegrationEvent>
{
    private readonly IProxyConfigProvider _configProvider;
    private readonly ILogger<ServiceInstanceRegisteredIntegrationEventConsumer> _logger;

    public ServiceInstanceRegisteredIntegrationEventConsumer(IProxyConfigProvider configProvider, ILogger<ServiceInstanceRegisteredIntegrationEventConsumer> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<ServiceInstanceRegisteredIntegrationEvent> context)
    {
        if (_configProvider is not InMemoryProxyConfigProvider inMemoryProvider)
            throw new NotSupportedException("Dynamic Routes and Clusters update is only supported for InMemoryProxyConfigProvider.");

        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;
        HashSet<string> addresses = context.Message.ServiceAddresses;

        if (instanceId.Equals(Guid.Empty))
            throw new InvalidDataException($"{nameof(context.Message.InstanceId)} must not be empty.");

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new InvalidDataException($"{nameof(context.Message.ServiceType)} must not be null or empty.");

        if (!(addresses?.Any() ?? false))
            throw new InvalidDataException($"{nameof(context.Message.ServiceAddresses)} must not be null or empty.");

        string addressesString = string.Join("; ", addresses);

        _logger.LogInformation("----- Handling {ServiceType} instance registered event: {InstanceId} - {Addresses}.", serviceType, instanceId, addressesString);

        var config = _configProvider.GetConfig();
        var newRoutes = config.Routes.ToList();
        var newClusters = config.Clusters.ToList();

        var oldCluster = config.Clusters.FirstOrDefault(cluster =>
            string.Equals(cluster.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase));

        var newDestinations = InMemoryProxyConfigProvider.GenerateDestinations(
            serviceType,
            new HashSet<ServiceInstance>() { new ServiceInstance(instanceId, addresses) });

        // If cluster for this service type already existed, merge existing destinations with new ones,
        // if cluster did not exist, generate new routes from scratch based on matching paths from config file
        if (oldCluster is not null)
        {
            newClusters.Remove(oldCluster);

            if ((oldCluster?.Destinations?.Count ?? 0) > 0)
                newDestinations = newDestinations.Union(oldCluster!.Destinations!).ToDictionary(k => k.Key, v => v.Value);
        }
        else
        {
            var paths = PathsConfig.GetMatchingPaths(serviceType);
            newRoutes.AddRange(InMemoryProxyConfigProvider.GenerateRoutes(serviceType, paths));
        }

        newClusters.Add(InMemoryProxyConfigProvider.GenerateCluster(serviceType, newDestinations));
        inMemoryProvider.Update(newRoutes, newClusters);

        return Task.CompletedTask;
    }
}