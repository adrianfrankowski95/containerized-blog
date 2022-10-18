namespace Blog.Services.Identity.API.Extensions;

public static class HttpContextExtensions
{
    private static readonly List<string> _forwardedHeaders = new() { "X-Forwarded-Proto", "X-Forwarded-Host", "X-Forwarded-Prefix" };

    public static string GetOriginalProtocol(this HttpContext httpContext) =>
        httpContext.Request.Headers?[_forwardedHeaders[0]].ToString()
        ?? throw new InvalidDataException($"Forwarded request is missing {_forwardedHeaders[0]} header.");

    public static string GetOriginalHost(this HttpContext httpContext) =>
        httpContext.Request.Headers?[_forwardedHeaders[1]].ToString()
        ?? throw new InvalidDataException($"Forwarded request is missing {_forwardedHeaders[1]} header.");

    public static string GetOriginalPrefix(this HttpContext httpContext) =>
        httpContext.Request.Headers?[_forwardedHeaders[2]].ToString()
        ?? throw new InvalidDataException($"Forwarded request is missing {_forwardedHeaders[2]} header.");

    public static string GetOriginalBaseRoute(this HttpContext httpContext) => GetOriginalProtocol(httpContext) + "://" + GetOriginalHost(httpContext);

    public static string GetOriginalRoute(this HttpContext httpContext) => GetOriginalBaseRoute(httpContext) + GetOriginalPrefix(httpContext);
}