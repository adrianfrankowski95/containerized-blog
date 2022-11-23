using Blog.Services.Discovery.API.Models;
using Blog.Services.Discovery.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddDiscoveryInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("DiscoveryDb")
            ?? throw new ArgumentNullException("Could not retrieve a connection string to Discovery db.");

        services.AddDbContextPool<DiscoveryDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("DiscoveryRegister")
                ?? throw new ArgumentNullException("Could not retrieve a connection string to a Discovery register."));


            redis.GetServer(redis.GetEndPoints().Single()).ConfigSet("notify-keyspace-events", "Ex");

            return redis;
        });

        services.AddOptions<ServiceRegistryOptions>().Bind(config.GetSection(ServiceRegistryOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddScoped<IServiceRegistry, RedisServiceRegistry>();
        services.AddHostedService<RedisKeyExpiredEventHandler>();

        return services;
    }
}