using System.Security.Cryptography.X509Certificates;

namespace Blog.Services.Auth.API.Services;

public interface ISigningCertificateManager
{
    public IEnumerable<X509Certificate2> GetOrGenerateCertificates();
    public IEnumerable<X509Certificate2> GenerateAndRegisterCertificates();
}