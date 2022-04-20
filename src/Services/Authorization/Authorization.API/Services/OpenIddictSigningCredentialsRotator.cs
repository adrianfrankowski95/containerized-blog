using Blog.Services.Auth.API.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Quartz;

namespace Blog.Services.Authorization.API.Services
{
    public class OpenIddictSigningCredentialsRotator : IJob
    {
        public static JobKey Id { get; } = new JobKey(
                name: nameof(OpenIddictSigningCredentialsRotator),
                group: typeof(OpenIddictSigningCredentialsRotator).Assembly.GetName().Name!);
        private readonly IServiceProvider _services;

        public OpenIddictSigningCredentialsRotator(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = _services.CreateAsyncScope();

            var certificateManager = scope.ServiceProvider.GetRequiredService<ISigningCertificateManager>();
            var certificates = certificateManager.GenerateAndRegisterCertificates();

            var options = scope.ServiceProvider.GetRequiredService<IOptions<OpenIddictServerOptions>>().Value;

            foreach (var cert in certificates)
            {
                var securityKey = new X509SecurityKey(cert);
                var signingCredential = new SigningCredentials(securityKey, securityKey.PrivateKey.SignatureAlgorithm);
                options.SigningCredentials.Add(signingCredential);
            }
        }
    }
}