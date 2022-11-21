#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Gateways.WebGateway.API.Configs;

public class UrlsConfig
{
    public const string Section = "Urls";

    [Required]
    public string DiscoveryService { get; set; }
}