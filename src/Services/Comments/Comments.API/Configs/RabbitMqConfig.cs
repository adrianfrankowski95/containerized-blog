using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Comments.API.Configs;

public class RabbitMqConfig
{
    public const string Section = "RabbitMq";

    [Required]
    public string Host { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public ushort Port { get; set; }

    [Required]
    public string VirtualHost { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    public const string QueueName = "comments-api";
}