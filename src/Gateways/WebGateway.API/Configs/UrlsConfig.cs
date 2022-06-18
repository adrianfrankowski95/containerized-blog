namespace Blog.Gateways.WebGateway.API.Configs;

public class UrlsConfig
{
    public const string Section = "Urls";

    public string AuthService { get; set; }
    public static class AuthActions
    {
        public static string Authorize() => $"/api/v1/auth/authorize";
        public static string Refresh() => "/api/v1/auth/refresh";
        public static string Revoke() => "/api/v1/auth/revoke";
        public static string RevokeUser(Guid userId) => $"/api/v1/auth/revoke/{userId}";
        public static string GetJwks() => $"/api/v1/auth/jwks";
        public static string GetDiscovery() => $"/api/v1/auth/discovery";
    }

    public string BloggingService { get; set; }

    public string CommentsService { get; set; }
    public string IdentityService { get; set; }
    public string EmailingService { get; set; }
    public string DiscoveryService { get; set; }
}