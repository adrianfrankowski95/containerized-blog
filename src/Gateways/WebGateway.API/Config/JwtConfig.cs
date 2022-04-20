namespace Blog.Gateways.WebGateway.API.Config;

public class JwtConfig
{
    public const string Section = "Jwt";
    public AccessTokenOptions AccessToken { get; set; }
    public RefreshTokenOptions RefreshToken { get; set; }
}

public interface ITokenCookieOptions
{
    public string CookieName { get; set; }
    public int CookieExpirationDays { get; set; }
    public string CookieDomain { get; set; }
}

public class AccessTokenOptions : ITokenCookieOptions
{
    public string Name { get; set; }
    public string Audience { get; set; }
    public string Authority { get; set; }
    public string Issuer { get; set; }
    public string CookieName { get; set; }
    public int CookieExpirationDays { get; set; }
    public string CookieDomain { get; set; }
}

public class RefreshTokenOptions : ITokenCookieOptions
{
    public string Name { get; set; }
    public string CookieName { get; set; }
    public int CookieExpirationDays { get; set; }
    public string CookieDomain { get; set; }
}