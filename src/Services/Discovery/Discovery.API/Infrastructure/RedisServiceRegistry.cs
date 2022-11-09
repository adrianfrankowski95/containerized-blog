using Blog.Services.Discovery.API.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure;

public class RedisServiceRegistry : IServiceRegistry
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _redisDb;
    private readonly IOptionsMonitor<ServiceRegistryOptions> _options;

    public RedisServiceRegistry(IConnectionMultiplexer redis, IOptionsMonitor<ServiceRegistryOptions> options)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _redisDb = redis.GetDatabase();
    }

    public Task<bool> RegisterServiceInstance(ServiceInstance serviceInstance)
    {
        if (serviceInstance is null)
            throw new ArgumentNullException(nameof(serviceInstance));

        return _redisDb.StringSetAsync(
            key: serviceInstance.Key.ToString(),
            value: string.Join(';', serviceInstance.Addresses),
            expiry: _options.CurrentValue.Expiry);
    }

    public Task<bool> UnregisterServiceInstance(ServiceInstance serviceInstance)
    {
        if (serviceInstance is null)
            throw new ArgumentNullException(nameof(serviceInstance));

        return _redisDb.KeyDeleteAsync(serviceInstance.Key.ToString());
    }

    public async Task<IList<ServiceInstance>> GetAllServiceInstances()
    {
        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(
            pattern: ServiceInstanceKey.GetAllInstancesKeyPattern());

        if (!(keys?.Any() ?? false))
            return Array.Empty<ServiceInstance>();

        var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

        if ((entries?.Length ?? 0) == 0)
            return Array.Empty<ServiceInstance>();

        var services = new List<ServiceInstance>();

        for (int i = 0; i < keys.Count(); ++i)
        {
            if (entries!.ElementAt(i).HasValue)
            {
                var key = ServiceInstanceKey.FromString(keys.ElementAt(i).ToString());
                var addresses = entries!.ElementAt(i).ToString().Split(';').ToHashSet();

                services.Add(new ServiceInstance(key, addresses));
            }
        }
        return services;
    }

    public async Task<IList<ServiceInstance>> GetServiceInstancesOfType(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(
            pattern: ServiceInstanceKey.GetServiceTypeKeyPattern(serviceType));

        if (!(keys?.Any() ?? false))
            return Array.Empty<ServiceInstance>();

        var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

        if ((entries?.Length ?? 0) == 0)
            return Array.Empty<ServiceInstance>();

        var services = new List<ServiceInstance>();

        for (int i = 0; i < keys.Count(); ++i)
        {
            if (entries!.ElementAt(i).HasValue)
            {
                var key = ServiceInstanceKey.FromString(keys.ElementAt(i).ToString());
                var addresses = entries!.ElementAt(i).ToString().Split(';').ToHashSet();

                services.Add(new ServiceInstance(key, addresses));
            }
        }
        return services;
    }

    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInstance serviceInstance)
    {
        if (serviceInstance is null)
            throw new ArgumentNullException(nameof(serviceInstance));

        return _redisDb.KeyExpireAsync(serviceInstance.Key.ToString(), _options.CurrentValue.Expiry);
    }
}
