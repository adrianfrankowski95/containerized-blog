using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Authorization.API.Config;

public class AuthOptions
{
    [Required]
    public string Issuer { get; set; }

    [Required]
    public string Authority { get; set; }
}