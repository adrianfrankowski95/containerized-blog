using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Models;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Services;

public class InMemoryProxyConfigProvider : IInMemoryProxyConfigProvider
{
    private volatile IProxyConfig _config;
    private CancellationTokenSource _cts;

    public InMemoryProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        _cts = new();
        _config = new InMemoryProxyConfig(routes, clusters, _cts.Token);
    }

    private InMemoryProxyConfigProvider()
    {
    }

    public static async Task<IInMemoryProxyConfigProvider> InitializeFromDiscoveryServiceAsync(IDiscoveryService discoveryService)
    {
        var instancesInfo = await discoveryService.GetAllInstanceInfoAsync().ConfigureAwait(false);

        if (instancesInfo is null)
            throw new InvalidOperationException("Error discovering services during reverse proxy initialization");

        if (instancesInfo.Any(x => x.Value.Any(a => a.Addresses is null || a.Addresses.Count == 0)))
            throw new InvalidOperationException("Error requesting addresses from discovery service");

        List<RouteConfig> routes = new();
        List<ClusterConfig> clusters = new();

        var provider = new InMemoryProxyConfigProvider();

        foreach (var serviceType in instancesInfo.Keys)
        {
            var paths = PathsConfig.GetServiceMatchingPaths(serviceType);
            provider.GenerateRoutes(serviceType, paths, ref routes);

            var destinations = provider.GenerateDestinations(instancesInfo[serviceType]);
            provider.GenerateCluster(serviceType, destinations, ref clusters);
        }

        return new InMemoryProxyConfigProvider(routes, clusters);
    }

    public void GenerateRoutes(string serviceType, IEnumerable<string> matchingPaths, ref List<RouteConfig> routes)
    {
        if (matchingPaths is null || !matchingPaths.Any())
            throw new ArgumentNullException(nameof(matchingPaths));

        int routeIndex = 1;
        foreach (var path in matchingPaths)
        {
            routes.Add(new RouteConfig
            {
                RouteId = serviceType + "-route-" + routeIndex,
                ClusterId = serviceType,
                Match = new RouteMatch { Path = path }
            });
            ++routeIndex;
        }
    }

    public Dictionary<string, DestinationConfig> GenerateDestinations(HashSet<ServiceInstanceInfo> instancesInfo)
    {
        Dictionary<string, DestinationConfig> destinations = new();

        foreach (var instanceInfo in instancesInfo)
        {
            int destinationIndex = 1;
            foreach (string address in instanceInfo.Addresses)
            {
                destinations.Add(instanceInfo + "-" + instanceInfo.InstanceId + "-" + destinationIndex,
                    new DestinationConfig { Address = address });

                ++destinationIndex;
            }
        };

        return destinations;
    }

    public void GenerateCluster(string clusterId, Dictionary<string, DestinationConfig> destinations, ref List<ClusterConfig> clusters)
    {
        clusters.Add(new ClusterConfig
        {
            ClusterId = clusterId,
            Destinations = destinations
        });
    }

    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        var newCts = new CancellationTokenSource();
        _config = new InMemoryProxyConfig(routes, clusters, newCts.Token);

        SignalChange();
        ReplaceChangeTokenSource(newCts);
    }

    public IProxyConfig GetConfig() => _config;

    private void SignalChange() => _cts.Cancel();

    private void ReplaceChangeTokenSource(CancellationTokenSource newCts)
        => _cts = newCts;
}