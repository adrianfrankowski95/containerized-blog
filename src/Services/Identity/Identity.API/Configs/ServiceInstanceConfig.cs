using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Identity.API.Configs;

public class ServiceInstanceConfig
{
    [Required]
    public Guid InstanceId { get; set; }

    [Required]
    public string ServiceType { get; set; }

    [Required]
    public TimeSpan HeartbeatInterval { get; set; }
}
