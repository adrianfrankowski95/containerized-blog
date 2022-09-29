using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Models;
using Yarp.ReverseProxy.Configuration;
using static Blog.Gateways.WebGateway.API.Configs.GatewayConstants;

namespace Blog.Gateways.WebGateway.API.Services;

public class InMemoryProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private volatile IProxyConfig _config;
    private CancellationTokenSource _changeToken;
    private bool _disposed;

    public InMemoryProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        _changeToken = new();
        _config = new InMemoryProxyConfig(routes, clusters, _changeToken.Token);
    }

    public static InMemoryProxyConfigProvider LoadFromDiscoveryService(IDiscoveryService discoveryService)
    {
        var services = discoveryService.GetServicesAsync().GetAwaiter().GetResult();

        if (services is null)
            throw new InvalidOperationException("Error discovering services during reverse proxy initialization.");

        var servicesWithoutAddresses = ServiceTypes
            .List()
            .Where(serviceType =>
                (services[serviceType]?.Count ?? 0) == 0 ||
                !(services[serviceType].SelectMany(instances => instances.Addresses)?.Any() ?? false));

        if (servicesWithoutAddresses.Any())
            throw new InvalidOperationException($"Error getting addresses of the following services: {string.Join(", ", servicesWithoutAddresses)}.");

        List<RouteConfig> routes = new();
        List<ClusterConfig> clusters = new();

        foreach (var serviceType in services.Keys)
        {
            var paths = PathsConfig.GetMatchingPaths(serviceType);
            routes.AddRange(GenerateRoutes(serviceType, paths));

            var destinations = GenerateDestinations(serviceType, services[serviceType]);
            clusters.Add(GenerateCluster(serviceType, destinations));
        }

        return new InMemoryProxyConfigProvider(routes, clusters);
    }

    public static IEnumerable<RouteConfig> GenerateRoutes(string serviceType, IEnumerable<(string incomingPath, string outgoingPath)> matchingPaths)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (matchingPaths?.Any() ?? true)
            throw new ArgumentNullException(nameof(matchingPaths));

        var routes = new List<RouteConfig>(matchingPaths.Count());

        int routeIndex = 1;
        foreach (var (incomingPath, outgoingPath) in matchingPaths)
        {
            routes.Add(new RouteConfig
            {
                RouteId = serviceType + "-route-" + routeIndex,
                ClusterId = serviceType,
                Match = new RouteMatch { Path = incomingPath },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>{ {"PathPattern", outgoingPath }}
                }
            });
            ++routeIndex;
        }

        return routes;
    }

    public static Dictionary<string, DestinationConfig> GenerateDestinations(string serviceType, IReadOnlySet<ServiceInstance> serviceInstances)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        Dictionary<string, DestinationConfig> destinations = new();

        foreach (var instanceInfo in serviceInstances)
        {
            int destinationIndex = 1;
            foreach (string address in instanceInfo.Addresses)
            {
                destinations.Add(serviceType + "-" + instanceInfo.InstanceId + "-" + destinationIndex,
                    new DestinationConfig { Address = address });

                ++destinationIndex;
            }
        };
        return destinations;
    }

    public static ClusterConfig GenerateCluster(string serviceType, IReadOnlyDictionary<string, DestinationConfig> destinations)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        return new ClusterConfig
        {
            ClusterId = serviceType,
            Destinations = destinations,
        };
    }

    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        var newChangeToken = new CancellationTokenSource();
        _config = new InMemoryProxyConfig(routes, clusters, newChangeToken.Token);

        SignalChange();
        ReplaceChangeToken(newChangeToken);
    }

    public IProxyConfig GetConfig() => _config;

    private void SignalChange() => _changeToken.Cancel();

    private void ReplaceChangeToken(CancellationTokenSource newCts)
        => _changeToken = newCts;

    public void Dispose()
    {
        if (!_disposed)
        {
            _changeToken?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}