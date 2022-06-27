using Blog.Gateways.WebGateway.API.Configs;
using Yarp.ReverseProxy.Configuration;

namespace Blog.Gateways.WebGateway.API.ConfigProviders;

public class InMemoryProxyConfigProvider : IProxyConfigProvider
{
    private volatile IProxyConfig _config;
    private CancellationTokenSource _cts;

    public InMemoryProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        _cts = new();
        _config = new InMemoryProxyConfig(routes, clusters, _cts.Token);
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