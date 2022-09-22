using Blog.Services.Discovery.API.Grpc;
using Blog.Services.Identity.API.Configs;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Infrastructure;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

var config = GetConfiguration(env);
builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services.AddRazorPages();

services
    .AddInstanceConfig()
    .AddNodaTime()
    .AddInfrastructure(config)
    .AddMassTransitRabbitMqBus(config)
    .AddGrpcDiscoveryService(config)
    .AddGrpcEmailingService()
    .AddDomainServices();

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

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseStaticFiles(); //html, css, images, js in wwwroot folder

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
        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "identity-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddGrpcDiscoveryService(this IServiceCollection services, IConfiguration config)
    {
        var address = config.GetRequiredSection(UrlsConfig.Section).Get<UrlsConfig>().DiscoveryService;

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
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = config.GetRequiredSection(RabbitMqConfig.Section).Get<RabbitMqConfig>();

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, opts =>
                {
                    opts.Username(rabbitMqConfig.Username);
                    opts.Password(rabbitMqConfig.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConfig.QueueName, opts =>
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

    public static IServiceCollection AddNodaTime(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock>(c => SystemClock.Instance);

        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.TryAddTransient<ISysTime, SysTime>();
        services.TryAddTransient<LoginService>();
        services.TryAddTransient<PasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}