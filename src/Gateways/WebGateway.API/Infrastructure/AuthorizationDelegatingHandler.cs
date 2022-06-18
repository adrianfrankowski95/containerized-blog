using System.Net.Http.Headers;
using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Blog.Gateways.WebGateway.API.Infrastructure;

public class AuthorizationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtConfig _jwtOptions;
    public AuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, IOptions<JwtConfig> jwtOptions)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var incomingRequest = _httpContextAccessor.HttpContext.Request;

        var accessToken = incomingRequest.GetTokenFromCookie(_jwtOptions.AccessToken);

        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
