#nullable disable

using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Emailing.API.Configs;

public class EmailConfig
{
    public const string Section = "Email";

    [Required]
    public string FromEmail { get; set; }
    [Required]
    public string FromName { get; set; }

    [Required]
    public string Host { get; set; }

    [Required]
    [Range(1, Int32.MaxValue)]
    public int Port { get; set; }

    [Required]
    public bool UseSsl { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    [Required]
    public bool RequireAuthentication { get; set; }

    [Required]
    public string SocketOptions { get; set; }
}