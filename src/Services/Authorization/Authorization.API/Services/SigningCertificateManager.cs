using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Blog.Services.Auth.API.Config;
using Microsoft.Extensions.Options;

namespace Blog.Services.Auth.API.Services;

public class SigningCertificateManager : ISigningCertificateManager
{
    private readonly IOptionsMonitor<SigningCertificateOptions> _config;

    public SigningCertificateManager(IOptionsMonitor<SigningCertificateOptions> config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public IEnumerable<X509Certificate2> GetOrGenerateCertificates()
    {
        var count = _config.CurrentValue.ActiveCertificatesCount;

        if (count <= 0)
            throw new ArgumentException($"Incorrect number of active signing certificates: {count}");

        IEnumerable<X509Certificate2> certificates = LoadCertificatesFromStore();

        var rotationInterval = _config.CurrentValue.RotationIntervalDays;

        List<X509Certificate2> validCertificates = certificates
            .Where(x => DateTime.UtcNow.Subtract(x.NotAfter).TotalDays >= rotationInterval)
            .ToList();

        if (validCertificates.Count < count)
        {
            int certificatesToGenerate = count - validCertificates.Count;

            GenerateAndRegisterCertificates(certificatesToGenerate, validCertificates);
        }

        return validCertificates;
    }

    public IEnumerable<X509Certificate2> GenerateAndRegisterCertificates()
    {
        var count = _config.CurrentValue.ActiveCertificatesCount;

        if (count <= 0)
            throw new ArgumentException($"Incorrect number of active signing certificates: {count}");

        List<X509Certificate2> certificates = new(count);
        GenerateAndRegisterCertificates(count, certificates);

        return certificates;
    }


    private void GenerateAndRegisterCertificates(int count, IList<X509Certificate2> certificates)
    {
        for (int i = 0; i < count; ++i)
        {
            var newCertificate = GenerateCertificate();
            RegisterCertificate(newCertificate);

            certificates.Add(newCertificate);
        }
    }

    private IEnumerable<X509Certificate2> LoadCertificatesFromStore()
    {
        var subject = _config.CurrentValue.Subject;

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentNullException(nameof(_config.CurrentValue.Subject));

        using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var certificates = store.Certificates.Find(
                findType: X509FindType.FindBySubjectDistinguishedName,
                findValue: new X500DistinguishedName($"CN={subject}"),
                validOnly: true)
                .OrderByDescending(x => x.NotAfter) ??
                Enumerable.Empty<X509Certificate2>();

        store.Close();

        return certificates;
    }

    private X509Certificate2 GenerateCertificate()
    {
        var subject = _config.CurrentValue.Subject;

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentNullException(nameof(_config.CurrentValue.Subject));

        using RSA rsa = RSA.Create(keySizeInBits: 2048);

        var distinguishedName = new X500DistinguishedName($"CN={subject}");
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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

        var certificate = request
            .CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(_config.CurrentValue.ExpirationYears));

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