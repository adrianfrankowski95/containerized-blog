using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.TryAddSingleton<IConnectionMultiplexer>(opts =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")));

        services.TryAddScoped<IServiceRegistry, RedisServiceRegistry>();

        return services;
    }
}