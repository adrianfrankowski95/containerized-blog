using Blog.Gateways.WebGateway.API.Config;

namespace Blog.Gateways.WebGateway.API.Extensions;

public static class HttpResponseExtensions
{
    public static void SetTokenCookie(this HttpResponse response, string token, bool isPersistent, ITokenCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Domain = options.CookieDomain
        };

        if (isPersistent)
            cookieOptions.Expires = DateTime.UtcNow.AddDays(options.CookieExpirationDays);

        response.Cookies.Append(options.CookieName, token, cookieOptions);
    }

    public static void ClearTokenCookie(this HttpResponse response, ITokenCookieOptions options)
        => response.Cookies.Delete(options.CookieName);
}
