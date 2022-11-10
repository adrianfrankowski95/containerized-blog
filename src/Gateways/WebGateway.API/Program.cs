using Blog.Gateways.WebGateway.API.Configs;
using Blog.Gateways.WebGateway.API.Integration.Consumers;
using Blog.Gateways.WebGateway.API.Services;
using Blog.Services.Discovery.API.Grpc;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = GetConfiguration(env);

builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services.AddSwaggerGen();
services
    .AddGrpcDiscoveryService(config)
    .AddYarp();

//await AuthContextSeed.SeedAsync(config);

var app = builder.Build();

if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy(); // Yarp

app.Run();

static IConfiguration GetConfiguration(IWebHostEnvironment env)
    => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcDiscoveryService(this IServiceCollection services, IConfiguration config)
    {
        var address = config.GetRequiredSection(UrlsConfig.Section).Get<UrlsConfig>().DiscoveryService;

        if (string.IsNullOrWhiteSpace(address))
            throw new InvalidOperationException($"{nameof(UrlsConfig.DiscoveryService)} URL must not be null.");

        services.AddGrpcClient<GrpcDiscoveryService.GrpcDiscoveryServiceClient>(opts =>
        {
            opts.Address = new Uri(address);
        });

        services.TryAddTransient<IDiscoveryService, DiscoveryService>();

        return services;
    }

    public static IServiceCollection AddYarp(this IServiceCollection services)
    {
        services.TryAddSingleton<IProxyConfigProvider>(sp =>
        {
            var discoveryService = sp.GetRequiredService<IDiscoveryService>();
            return InMemoryProxyConfigProvider.LoadFromDiscoveryService(discoveryService);
        });

        services.AddReverseProxy();

        return services;
    }

    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<RabbitMqConfig>()
            .Bind(config.GetRequiredSection(RabbitMqConfig.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<ServiceInstanceRegisteredIntegrationEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = config.GetRequiredSection(RabbitMqConfig.Section).Get<RabbitMqConfig>();

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.VirtualHost, opts =>
                {
                    opts.Username(rabbitMqConfig.Username);
                    opts.Password(rabbitMqConfig.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConfig.QueueName, opts =>
                {
                    opts.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
                    opts.ConfigureConsumers(context);
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
            });

        return services;
    }

    // public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration config)
    // {
    //     services.AddOptions<JwtConfig>().Bind(config.GetRequiredSection(JwtConfig.Section));
    //     services
    //         .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    //         .Configure<IOptions<JwtConfig>, IOptions<ServicesConfig>>((opts, jwtOptions, urlsOptions) =>
    //         {
    //             var accessTokenOptions = jwtOptions.Value.AccessToken;

    //             opts.MapInboundClaims = false;
    //             opts.RequireHttpsMetadata = true;
    //             opts.Authority = accessTokenOptions.Authority;
    //             opts.ClaimsIssuer = accessTokenOptions.Issuer;
    //             opts.MetadataAddress = ServicesConfig.AuthActions.GetDiscovery();

    //             opts.TokenValidationParameters = new TokenValidationParameters
    //             {
    //                 RequireSignedTokens = true,
    //                 RequireExpirationTime = true,
    //                 ValidateLifetime = true,
    //                 ValidateIssuer = true,
    //                 ValidateAudience = true,
    //                 ValidateIssuerSigningKey = true,
    //                 ValidIssuer = accessTokenOptions.Issuer,
    //                 ValidAudience = accessTokenOptions.Audience,
    //                 ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
    //                 ClockSkew = TimeSpan.Zero,
    //                 NameClaimType = UserClaimTypes.Name,
    //                 RoleClaimType = UserClaimTypes.Role,
    //                 AuthenticationType = JwtBearerDefaults.AuthenticationScheme
    //             };
    //             opts.Events = new JwtBearerEvents()
    //             {
    //                 OnMessageReceived = context =>
    //                 {
    //                     context.Token = context.HttpContext.Request.GetTokenFromCookie(accessTokenOptions);

    //                     return Task.CompletedTask;
    //                 }
    //             };

    //         });

    //     services
    //         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //         .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => { });

    //     return services;
    // }
}