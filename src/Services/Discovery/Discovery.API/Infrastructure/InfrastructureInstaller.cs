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
        string connectionString = config.GetConnectionString("Postgres");

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
            var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
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