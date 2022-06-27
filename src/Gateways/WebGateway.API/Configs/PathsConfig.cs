namespace Blog.Gateways.WebGateway.API.Configs;

public static class PathsConfig
{
    private static readonly IDictionary<string, IEnumerable<string>> _paths = new Dictionary<string, IEnumerable<string>>()
    {
        { GatewayConstants.ServiceTypes.BloggingApi, new[] { "/api/posts/", "/api/tags/" } },
        { GatewayConstants.ServiceTypes.CommentsApi, new[] { "/api/comments/" } },
        { GatewayConstants.ServiceTypes.EmailingApi, new[] { "/api/emailing/" } },
        { GatewayConstants.ServiceTypes.IdentityApi, new[] { "/account/", "/connect/", "/.well-known/" } },
    };

    public static IEnumerable<string> GetServiceMatchingPaths(string serviceType) => _paths[serviceType];
}