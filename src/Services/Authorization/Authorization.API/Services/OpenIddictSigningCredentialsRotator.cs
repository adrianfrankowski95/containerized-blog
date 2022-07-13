using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Quartz;

namespace Blog.Services.Authorization.API.Services
{
    public class OpenIddictSigningCredentialsRotator : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<OpenIddictServerOptions> _optionsAccessor;

        public static JobKey Id { get; } = new JobKey(
                name: nameof(OpenIddictSigningCredentialsRotator),
                group: typeof(OpenIddictSigningCredentialsRotator).Assembly.GetName().Name!);


        public OpenIddictSigningCredentialsRotator(IOptionsMonitor<OpenIddictServerOptions> optionsAccessor, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var certificateManager = scope.ServiceProvider.GetRequiredService<ISigningCertificateManager>();
            var certificates = certificateManager.GenerateAndRegisterCertificates();

            var options = _optionsAccessor.CurrentValue;

            foreach (var cert in certificates)
            {
                var securityKey = new X509SecurityKey(cert);
                var signingCredential = new SigningCredentials(securityKey, securityKey.PrivateKey.SignatureAlgorithm);
                options.SigningCredentials.Add(signingCredential);
            }

            await scope.DisposeAsync().ConfigureAwait(false);
        }
    }
}