using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Discovery.API.Models;

public class ServiceRegistryOptions
{
    public const string Section = "ServiceRegistry";

    [Required]
    public TimeSpan Expiry { get; set; } = TimeSpan.FromSeconds(20);
}
