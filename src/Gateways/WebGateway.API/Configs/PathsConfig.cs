namespace Blog.Gateways.WebGateway.API.Configs;

public static class PathsConfig
{
    private static readonly IReadOnlyDictionary<string, IEnumerable<(string, string)>> _paths =
        new Dictionary<string, IEnumerable<(string, string)>>()
        {
            {
                GatewayConstants.ServiceTypes.BloggingApi,
                new[] { ("/posts{parameters}", "/api/v1/posts{parameters}"), ("/tags{parameters}", "/api/v1/tags{parameters}") }
            },
            {
                GatewayConstants.ServiceTypes.CommentsApi,
                new[] { ("/comments{parameters}",  "/api/v1/comments{parameters}") }
            },
            {
                GatewayConstants.ServiceTypes.EmailingApi,
                new[] { ("/emailing{parameters}", "/api/v1/emailing{parameters}") }
            },
            {
                GatewayConstants.ServiceTypes.IdentityApi,
                new[]
                {
                    ("/account{parameters}", "/account{parameters}"),
                    ("/connect{parameters}", "/connect{parameters}"),
                    ("/.well-known{parameters}", "/.well-known{parameters}")
                }
            },
        };

    public static IEnumerable<(string incomingPath, string outgoingPath)> GetMatchingPaths(string serviceType) => _paths[serviceType];
}