namespace Blog.Services.Auth.API.Config;

public class SigningCertificateConfig
{
    public const string Section = "SigningCertificate";
    public string Subject { get; set; }
    public int ExpirationYears { get; set; }
    public int RotationIntervalDays { get; set; }
    public int ActiveCertificatesCount { get; set; }
}