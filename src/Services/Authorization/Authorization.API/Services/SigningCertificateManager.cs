using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Blog.Services.Auth.API.Config;
using Microsoft.Extensions.Options;

namespace Blog.Services.Auth.API.Services;

public class SigningCertificateManager : ISigningCertificateManager
{
    private readonly SigningCertificateConfig _config;

    public SigningCertificateManager(IOptions<SigningCertificateConfig> config)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));

        if (_config.ActiveCertificatesCount <= 0)
            throw new ArgumentException($"Incorrect number of active signing certificates: {_config.ActiveCertificatesCount}");
    }

    public IEnumerable<X509Certificate2> GetOrGenerateCertificates()
    {
        IEnumerable<X509Certificate2> certificates = LoadCertificatesFromStore();

        List<X509Certificate2> validCertificates = certificates
            .Where(x => DateTime.UtcNow.Subtract(x.NotAfter).TotalDays >= _config.RotationIntervalDays)
            .ToList();

        if (validCertificates.Count <= _config.ActiveCertificatesCount)
        {
            int certificatesToGenerate = _config.ActiveCertificatesCount - validCertificates.Count;

            for (int i = 0; i < certificatesToGenerate; ++i)
            {
                var newCertificate = GenerateCertificate();
                RegisterCertificate(newCertificate);

                validCertificates.Add(newCertificate);
            }
        }

        return validCertificates;
    }

    public IEnumerable<X509Certificate2> GenerateAndRegisterCertificates()
    {
        List<X509Certificate2> certificates = new();

        for (int i = 0; i < _config.ActiveCertificatesCount; ++i)
        {
            var newCertificate = GenerateCertificate();
            RegisterCertificate(newCertificate);

            certificates.Add(newCertificate);
        }

        return certificates;
    }

    private IEnumerable<X509Certificate2> LoadCertificatesFromStore()
    {
        using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var certificates = store.Certificates.Find(
                findType: X509FindType.FindBySubjectDistinguishedName,
                findValue: new X500DistinguishedName($"CN={_config.Subject}"),
                validOnly: true)
                .OrderByDescending(x => x.NotAfter) ??
                Enumerable.Empty<X509Certificate2>();

        store.Close();

        return certificates;
    }

    private X509Certificate2 GenerateCertificate()
    {
        using RSA rsa = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName($"CN={_config.Subject}");
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            keyUsages: X509KeyUsageFlags.DigitalSignature,
            critical: true));

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(
            certificateAuthority: false,
            hasPathLengthConstraint: false,
            pathLengthConstraint: 0,
            critical: true));

        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(
            key: request.PublicKey,
            critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(_config.ExpirationYears));

        var certificateToExport = new X509Certificate2(
            rawData: certificate.Export(X509ContentType.Cert),
            password: string.Empty,
            keyStorageFlags: X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable)
            .CopyWithPrivateKey(rsa);

        return certificateToExport;
    }

    private static void RegisterCertificate(X509Certificate2 certificate)
    {
        using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);
        store.Close();
    }
}