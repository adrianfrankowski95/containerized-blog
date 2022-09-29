namespace Blog.Services.Identity.API.Extensions;

public static class HttpContextExtensions
{
    private static readonly List<string> _forwardedHeaders = new() { "X-Forwarded-Proto", "X-Forwarded-Host", "X-Forwarded-Prefix" };
    public static string GetOriginalRoute(this HttpContext httpContext)
    {
        var missingHeaders = new List<string>();
        var forwardedValues = _forwardedHeaders.Select(h =>
        {
            var headerValue = httpContext.Request.Headers[h].ToString();
            if (string.IsNullOrWhiteSpace(headerValue))
                missingHeaders.Add(h);

            return headerValue;
        });

        return missingHeaders.Any()
            ? throw new InvalidDataException($"Forwarded request is missing the following headers: {string.Join(", ", missingHeaders)}.")
            : forwardedValues.ElementAt(0) + "://" + forwardedValues.ElementAt(1) + forwardedValues.ElementAt(2);
    }
}