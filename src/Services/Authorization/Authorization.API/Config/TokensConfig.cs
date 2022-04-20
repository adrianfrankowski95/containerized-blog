namespace Blog.Services.Auth.API.Config;

public class TokensConfig
{
    public const string Section = "Tokens";
    public IdentityTokenOptions IdentityToken { get; set; }
    public AccessTokenOptions AccessToken { get; set; }
    public RefreshTokenOptions RefreshToken { get; set; }
}

public class IdentityTokenOptions
{
    public string Name { get; set; }
    public int ExpirationHours { get; set; }
    public string Audience { get; set; }
}

public class AccessTokenOptions
{
    public string Name { get; set; }
    public int ExpirationMinutes { get; set; }
    public Audiences Audiences { get; set; }
}

public class Audiences
{
    public string BloggingService { get; set; }
    public string UsersService { get; set; }
    public string CommentsService { get; set; }
    public string AuthService { get; set; }
    public string EmailingService { get; set; }
}

public class RefreshTokenOptions
{
    public string Name { get; set; }
    public int ExpirationDays { get; set; }
    public string Audience { get; set; }
    public string HmacSecretKey { get; set; }
}