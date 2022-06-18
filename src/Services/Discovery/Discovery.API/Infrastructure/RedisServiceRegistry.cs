using Blog.Services.Discovery.API.Models;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure;

public class RedisServiceRegistry : IServiceRegistry
{
    private readonly IDatabase _redisDb;

    public RedisServiceRegistry(IConnectionMultiplexer redis)
    {
        _redisDb = redis is null ? throw new ArgumentNullException(nameof(redis)) : redis.GetDatabase();
    }

    public async Task<(long changes, ServiceInfo updatedServiceInfo)> RegisterService(ServiceInfo serviceInfo)
    {
        if (serviceInfo is null)
            throw new ArgumentNullException(nameof(serviceInfo));

        var changes = await _redisDb.SetAddAsync(serviceInfo.Id, serviceInfo.Urls.Cast<RedisValue>().ToArray())
            .ConfigureAwait(false);

        if (changes > 0)
        {
            var newValues = await _redisDb.SetMembersAsync(serviceInfo.Id).ConfigureAwait(false);
            serviceInfo.Urls = newValues.Length > 0 ? newValues.Cast<string>() : Enumerable.Empty<string>();
        }

        return (changes, serviceInfo);
    }

    public async Task<(long changes, ServiceInfo updatedServiceInfo)> UnregisterService(ServiceInfo serviceInfo)
    {
        if (serviceInfo is null)
            throw new ArgumentNullException(nameof(serviceInfo));

        var changes = await _redisDb.SetRemoveAsync(serviceInfo.Id, serviceInfo.Urls.Cast<RedisValue>().ToArray())
            .ConfigureAwait(false);

        if (changes > 0)
        {
            var newValues = await _redisDb.SetMembersAsync(serviceInfo.Id).ConfigureAwait(false);
            serviceInfo.Urls = newValues.Length > 0 ? newValues.Cast<string>() : Enumerable.Empty<string>();
        }

        return (changes, serviceInfo);
    }

    public async Task<IEnumerable<ServiceInfo>> GetRegisteredServicesAsync()
    {
        var serviceTypes = Enum.GetValues<ServiceType>();
        var services = new List<ServiceInfo>();

        foreach (var type in serviceTypes)
        {
            var result = await _redisDb.SetMembersAsync(type.ToString()).ConfigureAwait(false);

            if (result.Length > 0)
            {
                services.Add(new ServiceInfo(type, result.Cast<string>()));
            }
        }

        return services;
    }

    public async Task<IEnumerable<string>> GetRegisteredServiceUrlsAsync(ServiceType serviceType)
    {
        var result = await _redisDb.SetMembersAsync(serviceType.ToString()).ConfigureAwait(false);

        return result.Length > 0 ? result.Cast<string>() : Enumerable.Empty<string>();
    }
}
