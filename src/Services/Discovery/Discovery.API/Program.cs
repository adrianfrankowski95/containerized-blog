using Blog.Services.Discovery.API.Configs;
using Blog.Services.Discovery.API.Grpc;
using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Integration.Consumers;
using Blog.Services.Discovery.API.Models;
using Discovery.API.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

var config = GetConfiguration(env);
builder.Configuration.AddConfiguration(config);

var services = builder.Services;

// Add services to the container.
services.AddLogging();
services.AddCors(opts =>
{
    opts.AddDefaultPolicy(
        x => x.SetIsOriginAllowed(origin => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()
        .AllowCredentials());
});

services
    .AddMassTransitRabbitMqBus(config)
    .AddRedisServiceRegistry(config)
    .AddGrpc();

var app = builder.Build();

app.UseCors();

app.MapGrpcService<DiscoveryService>();

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
    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<ServiceInstanceStartedEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                services
                    .AddOptions<RabbitMqConfig>()
                    .Bind(config.GetRequiredSection(RabbitMqConfig.Section))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                var rabbitMqConfig = config.GetValue<RabbitMqConfig>(RabbitMqConfig.Section);

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, opts =>
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

    public static IServiceCollection AddRedisServiceRegistry(this IServiceCollection services, IConfiguration config)
    {
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
            redis.GetServer(redis.GetEndPoints().Single()).ConfigSet("notify-keyspace-events", "Ex");

            return redis;
        });

        services.AddOptions<ServiceRegistryOptions>().Bind(config.GetSection(ServiceRegistryOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddScoped<IServiceRegistry, RedisServiceRegistry>();

        services.AddHostedService<RedisKeyExpiredEventNotifier>();

        return services;
    }
}