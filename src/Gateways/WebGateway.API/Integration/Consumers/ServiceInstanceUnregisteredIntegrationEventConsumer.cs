// Namespace of the event must be the same in Producers and in Consumers to make it work through MassTransit

using Blog.Gateways.WebGateway.API.Services;
using Blog.Integration.Events;
using MassTransit;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Integration.Consumers;

public class ServiceInstanceUnregisteredIntegrationEventConsumer : IConsumer<ServiceInstanceUnregisteredIntegrationEvent>
{
    private readonly IProxyConfigProvider _configProvider;
    private readonly ILogger<ServiceInstanceUnregisteredIntegrationEventConsumer> _logger;

    public ServiceInstanceUnregisteredIntegrationEventConsumer(IProxyConfigProvider configProvider, ILogger<ServiceInstanceUnregisteredIntegrationEventConsumer> logger)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<ServiceInstanceUnregisteredIntegrationEvent> context)
    {
        if (_configProvider is not InMemoryProxyConfigProvider inMemoryProvider)
            throw new NotSupportedException("Dynamic Routes and Clusters update is only supported for InMemoryProxyConfigProvider.");

        Guid instanceId = context.Message.InstanceId;
        string serviceType = context.Message.ServiceType;

        if (instanceId.Equals(Guid.Empty))
            throw new InvalidDataException($"{nameof(context.Message.InstanceId)} must not be empty.");

        if (string.IsNullOrWhiteSpace(serviceType))
            throw new InvalidDataException($"{nameof(context.Message.ServiceType)} must not be null or empty.");

        _logger.LogInformation("----- Handling {ServiceType} instance unregistered event: {InstanceId}.", serviceType, instanceId);

        var config = _configProvider.GetConfig();
        var newRoutes = config.Routes.ToList();
        var newClusters = config.Clusters.ToList();

        var oldCluster = config.Clusters.FirstOrDefault(cluster =>
            string.Equals(cluster.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase));

        bool clusterRecreated = false;
        if (oldCluster is not null)
        {
            newClusters.Remove(oldCluster);

            if ((oldCluster?.Destinations?.Count ?? 0) > 0)
            {
                var newDestinations = oldCluster!.Destinations!.Except(
                    oldCluster!.Destinations!.Where(k => k.Key.Contains(serviceType + "-" + instanceId.ToString())))
                    .ToDictionary(k => k.Key, v => v.Value);

                if (newDestinations?.Any() ?? false)
                {
                    newClusters.Add(InMemoryProxyConfigProvider.GenerateCluster(oldCluster.ClusterId, newDestinations));
                    clusterRecreated = true;
                }
            }
        }

        if (!clusterRecreated)
            newRoutes.RemoveAll(route => string.Equals(route.ClusterId, serviceType, StringComparison.OrdinalIgnoreCase));

        inMemoryProvider.Update(newRoutes, newClusters);

        return Task.CompletedTask;
    }
}