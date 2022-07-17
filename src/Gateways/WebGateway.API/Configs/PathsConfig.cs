namespace Blog.Gateways.WebGateway.API.Configs;

public static class PathsConfig
{
    private static readonly IDictionary<string, IEnumerable<string>> _paths = new Dictionary<string, IEnumerable<string>>()
    {
        {
            GatewayConstants.ServiceTypes.BloggingApi,
            new[] { "/api/{version}/posts{parameters}", "/api/{version}/tags{parameters}" }
        },
        {
            GatewayConstants.ServiceTypes.CommentsApi,
            new[] { "/api/{version}/comments{parameters}" } },
        {
            GatewayConstants.ServiceTypes.EmailingApi,
            new[] { "/api/{version}/emailing{parameters}" }
        },
        {
            GatewayConstants.ServiceTypes.IdentityApi,
            new[] { "/account/", "/connect/", "/.well-known/" }
        },
    };

    public static IEnumerable<string> GetServiceMatchingPaths(string serviceType) => _paths[serviceType];
}