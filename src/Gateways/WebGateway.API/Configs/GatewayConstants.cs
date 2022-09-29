namespace Blog.Gateways.WebGateway.API.Configs;

public static class GatewayConstants
{
    public static class ServiceTypes
    {
        public const string BloggingApi = "blogging-api";
        public const string CommentsApi = "comments-api";
        public const string EmailingApi = "emailing-api";
        public const string IdentityApi = "identity-api";
        public static IEnumerable<string> List() => new[] { BloggingApi, CommentsApi, EmailingApi, IdentityApi };
    }
}