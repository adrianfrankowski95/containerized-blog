#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Blogging.API.Options;

public class JwtOptions
{
    public const string Section = "Jwt";
    [Required]
    public string Issuer { get; set; }
    [Required]
    public string Audience { get; set; }
}
