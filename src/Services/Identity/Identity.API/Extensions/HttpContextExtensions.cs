namespace Blog.Services.Identity.API.Extensions;

public static class HttpContextExtensions
{
    public static string GetBaseRequestUri(this HttpContext context)
        => new Uri(context.Request.Scheme + context.Request.Host).AbsolutePath;
}