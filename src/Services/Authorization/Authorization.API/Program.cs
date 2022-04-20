using Blog.Services.Auth.API.Config;
using Blog.Services.Auth.API.Controllers;
using Blog.Services.Auth.API.Infrastructure;
using Blog.Services.Auth.API.Services;
using Blog.Services.Authorization.API.Models;
using Blog.Services.Authorization.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

bool isDevelopment = builder.Environment.IsDevelopment();
var config = GetConfiguration(isDevelopment);

var services = builder.Services;

// Add services to the container.
services.AddSwaggerGen();

services
    .AddAuthControllers(isDevelopment)
    //.AddCustomJwtAuthentication(config)
    .AddConfiguredQuartz()
    .AddSigningCertificatesManagement(config)
    .AddInfrastructureForOpenIddict(config)
    .AddConfiguredOpenIddict(config)
    .AddCustomServices();

//await AuthContextSeed.SeedAsync(config);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static IConfiguration GetConfiguration(bool isDevelopment)
{
    string configFileName = isDevelopment ? "appsettings.Development.json" : "appsettings.json";

    return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileName, optional: false, reloadOnChange: true)
                .Build();
}
static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<AuthConfig>().Bind(config);

        services.AddOptions<TokensConfig>().Bind(config.GetRequiredSection(TokensConfig.Section));

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<TokensConfig>, IOptions<AuthConfig>, IssuerSigningKey>((opts, jwtOptions, authOptions, signingKey) =>
            {
                var accessTokenOptions = jwtOptions.Value.AccessToken;
                var authOpts = authOptions.Value;

                opts.MapInboundClaims = false;
                opts.ClaimsIssuer = authOpts.Issuer;
                opts.RequireHttpsMetadata = true;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authOpts.Issuer,
                    ValidAudience = accessTokenOptions.Audiences.AuthService,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                    IssuerSigningKey = signingKey.Get(),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = UserClaimTypes.Name,
                    RoleClaimType = UserClaimTypes.Role,
                    AuthenticationType = JwtBearerDefaults.AuthenticationScheme
                };
            });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => { });

        return services;
    }

    public static IServiceCollection AddSigningCertificatesManagement(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<SigningCertificateConfig>()
            .Bind(config.GetRequiredSection(SigningCertificateConfig.Section));

        var signingCertificateConfig = config.GetRequiredSection(SigningCertificateConfig.Section)
            .Get<SigningCertificateConfig>();

        services.TryAddTransient<ISigningCertificateManager, SigningCertificateManager>();
        services.TryAddTransient<OpenIddictSigningCredentialsRotator>();

        services.Configure<QuartzOptions>(opts =>
        {
            opts.AddJob<OpenIddictSigningCredentialsRotator>(builder =>
            {
                builder.WithIdentity(OpenIddictSigningCredentialsRotator.Id)
                    .WithDescription("Job for rotating Signing Certificates and adding them to OpenIddict collection");
            });

            opts.AddTrigger(builder =>
            {
                builder.ForJob(OpenIddictSigningCredentialsRotator.Id)
                    .WithSimpleSchedule(opts =>
                        opts
                            .WithInterval(TimeSpan.FromDays(signingCertificateConfig.RotationIntervalDays))
                            .RepeatForever())
                    .WithDescription($"Trigger for {typeof(OpenIddictSigningCredentialsRotator).Name}")
                    .StartAt(DateTimeOffset.UtcNow.AddDays(signingCertificateConfig.RotationIntervalDays));
            });
        });

        return services;
    }

    public static IServiceCollection AddConfiguredQuartz(this IServiceCollection services)
    {
        services.AddQuartz(opts =>
        {
            opts.UseMicrosoftDependencyInjectionJobFactory();
            opts.UseSimpleTypeLoader();
            opts.UseInMemoryStore();
        });

        services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection AddConfiguredOpenIddict(this IServiceCollection services, IConfiguration config)
    {
        var tokensConfig = config.GetRequiredSection(TokensConfig.Section).Get<TokensConfig>();
        var authConfig = config.Get<AuthConfig>();

        services.AddOptions<OpenIddictServerOptions>()
                .Configure<ISigningCertificateManager>((opts, certificateManager) =>
                {
                    var certificates = certificateManager.GetOrGenerateCertificates();

                    foreach (var cert in certificates)
                    {
                        var securityKey = new X509SecurityKey(cert);
                        var signingCredential = new SigningCredentials(securityKey, securityKey.PrivateKey.SignatureAlgorithm);
                        opts.SigningCredentials.Add(signingCredential);
                    }
                });

        services.AddOpenIddict()
                .AddCore(opts =>
                {
                    opts
                        .UseEntityFrameworkCore()
                        .UseDbContext<AuthDbContext>()
                        .ReplaceDefaultEntities<Guid>();

                    opts.UseQuartz();
                })
                .AddServer(opts =>
                {
                    opts
                        .UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableVerificationEndpointPassthrough();

                    opts
                        .SetIssuer(new Uri(authConfig.Issuer))
                        .AllowRefreshTokenFlow()
                        .AllowPasswordFlow()
                        .SetIdentityTokenLifetime(TimeSpan.FromHours(tokensConfig.IdentityToken.ExpirationHours))
                        .SetAccessTokenLifetime(TimeSpan.FromMinutes(tokensConfig.AccessToken.ExpirationMinutes))
                        .SetRefreshTokenLifetime(TimeSpan.FromDays(tokensConfig.RefreshToken.ExpirationDays))
                        .DisableSlidingRefreshTokenExpiration()
                        .SetTokenEndpointUris("/connect/token")
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetRevocationEndpointUris("/connect/revoke")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetVerificationEndpointUris("/connect/verify")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetConfigurationEndpointUris("/.well-known/openid-configuration")
                        .SetCryptographyEndpointUris("/.well-known/openid-configuration/jwks")
                        .RegisterScopes(
                            OpenIddictConstants.Scopes.OpenId,
                            OpenIddictConstants.Scopes.Profile,
                            OpenIddictConstants.Permissions.Prefixes.Scope + Constants.Scopes.BloggingApi,
                            OpenIddictConstants.Permissions.Prefixes.Scope + Constants.Scopes.AuthApi,
                            OpenIddictConstants.Permissions.Prefixes.Scope + Constants.Scopes.CommentsApi,
                            OpenIddictConstants.Permissions.Prefixes.Scope + Constants.Scopes.EmailingApi,
                            OpenIddictConstants.Permissions.Prefixes.Scope + Constants.Scopes.UsersApi)
                        .RegisterClaims(
                            Constants.UserClaims.Email,
                            Constants.UserClaims.Id,
                            Constants.UserClaims.Name,
                            Constants.UserClaims.Role,
                            Constants.UserClaims.IsPersistent
                        );
                });

        return services;
    }
}