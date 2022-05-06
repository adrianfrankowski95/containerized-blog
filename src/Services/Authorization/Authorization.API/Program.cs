using Blog.Services.Auth.API.Config;
using Blog.Services.Auth.API.Controllers;
using Blog.Services.Auth.API.Infrastructure;
using Blog.Services.Auth.API.Services;
using Blog.Services.Authorization.API.Models;
using Blog.Services.Authorization.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quartz;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

bool isDevelopment = builder.Environment.IsDevelopment();
var config = GetConfiguration(isDevelopment);

var services = builder.Services;

// Add services to the container.
services
    .AddSwaggerGen()
    .AddRazorPages();

services
    .AddAuthControllers(isDevelopment)
    //.AddCustomJwtAuthentication(config)
    .AddConfiguredQuartz()
    .AddSigningCertificatesManagement(config)
    .AddInfrastructureForOpenIddict(config)
    .AddConfiguredOpenIddict(config)
    .AddCustomServices();
//.AddAntiforgery(); //added with Razor Pages

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
app.MapRazorPages();
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
        services.AddOptions<AuthOptions>()
            .Bind(config)
            .ValidateDataAnnotations()
            .ValidateOnStart(); ;

        services.AddOptions<TokensOptions>()
            .Bind(config.GetRequiredSection(TokensOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<TokensOptions>, IOptions<AuthOptions>, IssuerSigningKey>((opts, jwtOptions, authOptions, signingKey) =>
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
        services.AddOptions<SigningCertificateOptions>()
            .Bind(config.GetRequiredSection(SigningCertificateOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddTransient<ISigningCertificateManager, SigningCertificateManager>();

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
        services.AddOptions<AuthOptions>()
            .Bind(config)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<TokensOptions>()
            .Bind(config.GetRequiredSection(TokensOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenIddictServerOptions>()
                .Configure<ISigningCertificateManager>((opts, certificateManager) =>
                {
                    if (certificateManager is null)
                        throw new ArgumentNullException(nameof(certificateManager));

                    var certificates = certificateManager.GetOrGenerateCertificates();

                    X509SecurityKey securityKey;
                    SigningCredentials signingCredentials;

                    foreach (var cert in certificates)
                    {
                        securityKey = new X509SecurityKey(cert);
                        signingCredentials = new SigningCredentials(securityKey, securityKey.PrivateKey.SignatureAlgorithm);
                        opts.SigningCredentials.Add(signingCredentials);
                    }
                });

        var authConfig = config.Get<AuthOptions>();
        var tokensConfig = config.GetRequiredSection(TokensOptions.Section).Get<TokensOptions>();

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
                            Constants.UserClaimTypes.Email,
                            Constants.UserClaimTypes.Id,
                            Constants.UserClaimTypes.Name,
                            Constants.UserClaimTypes.EmailConfirmed,
                            Constants.UserClaimTypes.SecurityStamp,
                            Constants.UserClaimTypes.Role,
                            Constants.UserClaimTypes.IsPersistent
                        );
                });

        services.TryAddTransient<OpenIddictSigningCredentialsRotator>();

        var signingCertificateConfig = config.GetRequiredSection(SigningCertificateOptions.Section)
            .Get<SigningCertificateOptions>();

        services.Configure<QuartzOptions>(opts =>
        {
            opts.AddJob<OpenIddictSigningCredentialsRotator>(builder =>
            {
                builder.WithIdentity(OpenIddictSigningCredentialsRotator.Id)
                    .WithDescription("A job responsible for rotating Signing Certificates and adding them to OpenIddict collection");
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

    public static IServiceCollection AddConfiguredIdentity(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<User, UserRole>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores();

        services.AddOptions<IdentityOptions>()
            .Bind(config.GetRequiredSection("IdentityOptions"))
            .Configure<AuthOptions>((opts, authConfig) =>
            {
                opts.ClaimsIdentity.EmailClaimType = Constants.UserClaimTypes.Email;
                opts.ClaimsIdentity.RoleClaimType = Constants.UserClaimTypes.Role;
                opts.ClaimsIdentity.SecurityStampClaimType = Constants.UserClaimTypes.SecurityStamp;
                opts.ClaimsIdentity.UserIdClaimType = Constants.UserClaimTypes.Id;
                opts.ClaimsIdentity.UserNameClaimType = Constants.UserClaimTypes.Name;
                opts.Lockout.AllowedForNewUsers = true;
                opts.SignIn.RequireConfirmedPhoneNumber = false;
                opts.Stores.ProtectPersonalData = true;
                opts.Tokens.AuthenticatorIssuer = authConfig.Issuer;
            });

        return services;
    }