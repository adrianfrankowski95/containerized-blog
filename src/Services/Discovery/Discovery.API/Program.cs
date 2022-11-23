using Blog.Services.Discovery.API.Grpc;
using Blog.Services.Discovery.API.Infrastructure;
using Blog.Services.Discovery.API.Integration.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

var config = GetConfiguration(env);
builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services
    .AddDiscoveryInfrastructure(config)
    .AddMassTransitRabbitMqBus(config)
    .AddGrpc();

var app = builder.Build();

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
            x.AddConsumersFromNamespaceContaining<ServiceInstanceStartedIntegrationEventConsumer>();

            x.AddEntityFrameworkOutbox<DiscoveryDbContext>(cfg =>
            {
                cfg.UsePostgres();
                cfg.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = config.GetConnectionString("EventBus")
                    ?? throw new InvalidOperationException("Could not get a connection string for RabbitMq");

                cfg.Host(new Uri(connectionString));

                cfg.ReceiveEndpoint("discovery-api", opts =>
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
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}