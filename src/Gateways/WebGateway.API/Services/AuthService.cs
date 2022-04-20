using Microsoft.IdentityModel.Tokens;

namespace Blog.Gateways.WebGateway.API.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<JsonWebKey> GetJwkAsync()
    {

    }
}
