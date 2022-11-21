#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Emailing.API.Configs;

public class InstanceConfig
{
    [Required]
    public Guid InstanceId { get; set; }

    [Required]
    public string ServiceType { get; set; }

    [Required]
    public TimeSpan HeartbeatInterval { get; set; }

    [Required]
    public string Hostname { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Port { get; set; }
}
