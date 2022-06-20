using Blog.Services.Discovery.API.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddServiceRegistry(this IServiceCollection services, IConfiguration config)
    {
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")));

        services.AddOptions<ServiceRegistryOptions>().Bind(config.GetSection(ServiceRegistryOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddScoped<IServiceRegistry, RedisServiceRegistry>();

        return services;
    }
}