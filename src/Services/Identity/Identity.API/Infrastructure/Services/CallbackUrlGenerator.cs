using System.Text;
using Blog.Services.Identity.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class CallbackUrlGenerator : ICallbackUrlGenerator
{
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CallbackUrlGenerator(IUrlHelper urlHelper, IHttpContextAccessor httpContextAccessor)
    {
        _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GeneratePasswordResetCallbackUrl(string passwordResetCode)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Error accessing an HTTP context.");
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetCode));

        return _urlHelper.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { encodedCode },
                protocol: httpContext.GetOriginalProtocol(),
                host: httpContext.GetOriginalHost())
            ?? throw new InvalidDataException("Could not generate a password reset callback URL.");
    }
}
