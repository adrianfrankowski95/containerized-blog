using Blog.Gateways.WebGateway.API.Models;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Services;

public interface IInMemoryProxyConfigProvider : IProxyConfigProvider
{
    public void GenerateRoutes(string serviceType, IEnumerable<(string incomingPath, string outgoingPath)> matchingPaths, ref List<RouteConfig> routes);

    public Dictionary<string, DestinationConfig> GenerateDestinations(string serviceType, HashSet<ServiceInstanceInfo> instancesInfo);

    public void GenerateCluster(string clusterId, Dictionary<string, DestinationConfig> destinations, ref List<ClusterConfig> clusters);

    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters);
}