using System.ComponentModel.DataAnnotations;

namespace Blog.Services.Auth.API.Config;

public class SigningCertificateOptions
{
    public const string Section = "SigningCertificate";

    [Required]
    public string Subject { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ExpirationYears { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int RotationIntervalDays { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ActiveCertificatesCount { get; set; }
}