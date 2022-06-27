using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.Configs;

public class InMemoryProxyConfig : IProxyConfig
{
    public InMemoryProxyConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        CancellationToken cancellationToken)
    {
        Routes = routes;
        Clusters = clusters;

        ChangeToken = new CancellationChangeToken(cancellationToken);
    }

    public IReadOnlyList<RouteConfig> Routes { get; }

    public IReadOnlyList<ClusterConfig> Clusters { get; }

    public IChangeToken ChangeToken { get; }
}