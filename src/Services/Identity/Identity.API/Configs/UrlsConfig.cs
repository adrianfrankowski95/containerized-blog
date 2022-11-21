#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Identity.API.Configs;

public class UrlsConfig
{
    public const string Section = "Urls";

    [Required]
    public string DiscoveryService { get; set; }
}