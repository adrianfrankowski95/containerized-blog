using Blog.Gateways.WebGateway.API.Configs;

namespace Blog.Gateways.WebGateway.API.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetTokenFromCookie(this HttpRequest request, ITokenCookieOptions options)
    {
        return request.Cookies[options.CookieName];
    }

}
