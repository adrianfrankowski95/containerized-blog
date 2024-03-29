using Blog.Services.Discovery.API.Grpc;
using Blog.Services.Identity.API.Application;
using Blog.Services.Identity.API.Configs;
using Blog.Services.Identity.API.Controllers;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Infrastructure;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var config = GetConfiguration(env);

builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services.AddRazorPages();

services
    .AddInstanceConfig()
    .AddControllers(env)
    .AddNodaTime()
    .AddCustomServices()
    .AddDomainServices()
    .AddApplicationServices(config)
    .AddIdentityInfrastructure(config)
    .AddMassTransitRabbitMqBus(config)
    .AddGrpcDiscoveryService(config)
    .AddGrpcEmailingService()
    .AddConfiguredQuartz()
    .AddConfiguredOpenIddict(env);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseGlobalExceptionHandler();

app.UseForwardedHeaders(); // Transforms x-forwarded- headers from reverse proxy to request's headers
app.UseStaticFiles(); // Html, css, images, js in wwwroot folder

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

static IConfiguration GetConfiguration(IWebHostEnvironment env)
    => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstanceConfig(this IServiceCollection services)
    {
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        int port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "-1");

        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "identity-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
            opts.Hostname = hostname;
            opts.Port = port;
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddGrpcDiscoveryService(this IServiceCollection services, IConfiguration config)
    {
        var address = config.GetRequiredSection(UrlsConfig.Section).Get<UrlsConfig>()?.DiscoveryService
            ?? throw new InvalidOperationException("Could not get the Discovery service URL.");

        if (string.IsNullOrWhiteSpace(address))
            throw new InvalidOperationException($"{nameof(UrlsConfig.DiscoveryService)} URL must not be null");

        services.AddGrpcClient<GrpcDiscoveryService.GrpcDiscoveryServiceClient>(opts =>
        {
            opts.Address = new Uri(address);
        });

        services.TryAddTransient<IDiscoveryService, DiscoveryService>();

        return services;
    }

    public static IServiceCollection AddGrpcEmailingService(this IServiceCollection services)
    {
        services.AddGrpcClient<GrpcDiscoveryService.GrpcDiscoveryServiceClient>((sp, opts) =>
        {
            var discoveryService = sp.GetRequiredService<IDiscoveryService>();
            opts.Address = new Uri(discoveryService.GetAddressOfServiceTypeAsync("emailing-api").GetAwaiter().GetResult());
        });

        services.TryAddTransient<IEmailingService, EmailingService>();

        return services;
    }

    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<IdentityDbContext>(cfg =>
            {
                cfg.UsePostgres();
                cfg.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = config.GetConnectionString("EventBus")
                    ?? throw new InvalidOperationException("Could not get a connection string for RabbitMq");

                cfg.Host(new Uri(connectionString));

                cfg.ReceiveEndpoint("identity-api", opts =>
                {
                    opts.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
                });

                cfg.Durable = true;
            });
        });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(opts =>
            {
                opts.WaitUntilStarted = true;
                opts.StartTimeout = TimeSpan.FromSeconds(10);
                opts.StopTimeout = TimeSpan.FromSeconds(30);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<RabbitMqLifetimeIntegrationEventsPublisher>();

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .TryAddScoped<IIdentityService, IdentityService>();

        services
            .TryAddScoped<ICallbackUrlGenerator, CallbackUrlGenerator>();

        return services;
    }

    public static IServiceCollection AddNodaTime(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock>(c => NodaTime.SystemClock.Instance);
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.TryAddTransient<ISysTime, SysTime>();
        services.TryAddTransient<LoginService>();
        services.TryAddTransient<PasswordHasher, BcryptPasswordHasher>();

        return services;
    }

    public static IServiceCollection AddConfiguredAuthentication(this IServiceCollection services, IWebHostEnvironment env)
    {
        services
            .AddAuthentication()
            .AddCookie(IdentityConstants.AuthenticationScheme, opts =>
            {
                opts.LoginPath = new PathString("/account/login");
                opts.Cookie.IsEssential = true;
                opts.Cookie.HttpOnly = true;
            });
        // .AddJwtBearer(IdentityConstants.AuthenticationScheme, opts =>
        // {
        //     opts.SaveToken = true;
        //     opts.RequireHttpsMetadata = !env.IsDevelopment();

        //     // Prevents changing claims names by the middleware
        //     opts.MapInboundClaims = false;

        //     opts.TokenValidationParameters = new()
        //     {
        //         ValidateIssuerSigningKey = true,
        //         ValidateLifetime = true,
        //         ValidateIssuer = true,
        //         ValidateAudience = true,

        //         // Enables using in-built IsInRole() or [Authorize(Roles = ...)]
        //         RoleClaimType = IdentityConstants.UserClaimTypes.Role,
        //         NameClaimType = IdentityConstants.UserClaimTypes.Username,
        //     };

        //     opts.Events = new()
        //     {

        //     };
        // });

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

    public static IServiceCollection AddConfiguredOpenIddict(this IServiceCollection services, IWebHostEnvironment env)
    {
        services
            .AddOpenIddict()
            .AddCore(opts =>
            {
                opts
                    .UseEntityFrameworkCore()
                    .UseDbContext<IdentityDbContext>()
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
                    .EnableUserinfoEndpointPassthrough();

                opts
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout")
                    .SetUserinfoEndpointUris("/connect/userinfo")
                    .SetConfigurationEndpointUris("/.well-known/openid-configuration")
                    .SetCryptographyEndpointUris("/.well-known/openid-configuration/jwks")
                    .RegisterClaims(IdentityConstants.UserClaimTypes.List());

                if (env.IsDevelopment())
                {
                    opts.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }
                else
                    throw new NotSupportedException("Please configure certificates for production environment.");
            });

        return services;
    }
}