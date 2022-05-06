using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Auth.API.Config;

public class TokensOptions
{
    public const string Section = "Tokens";

    [Required]
    public IdentityTokenOptions IdentityToken { get; set; }

    [Required]
    public AccessTokenOptions AccessToken { get; set; }

    [Required]
    public RefreshTokenOptions RefreshToken { get; set; }
}

public class IdentityTokenOptions
{
    [Required]
    public string Name { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ExpirationHours { get; set; }

    [Required]
    public string Audience { get; set; }
}

public class AccessTokenOptions
{
    [Required]
    public string Name { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; set; }

    [Required]
    public Audiences Audiences { get; set; }
}

public class Audiences
{
    [Required]
    public string BloggingService { get; set; }

    [Required]
    public string UsersService { get; set; }

    [Required]
    public string CommentsService { get; set; }

    [Required]
    public string AuthService { get; set; }

    [Required]
    public string EmailingService { get; set; }
}

public class RefreshTokenOptions
{
    [Required]
    public string Name { get; set; }

    [Range(1, int.MaxValue)]
    public int ExpirationDays { get; set; }

    [Required]
    public string Audience { get; set; }

    [Required]
    public string HmacSecretKey { get; set; }
}