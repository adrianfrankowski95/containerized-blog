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

        return _redisDb.StringSetAsync(serviceInstance.Key, string.Join(';', serviceInstance.Addresses), _options.CurrentValue.Expiry);
    }

    public Task<bool> UnregisterServiceInstance(ServiceInstance serviceInstance)
    {
        if (serviceInstance is null)
            throw new ArgumentNullException(nameof(serviceInstance));

        return _redisDb.KeyDeleteAsync(serviceInstance.Key);
    }

    public async Task<IList<ServiceInstance>> GetAllServiceInstances()
    {
        //key pattern: services:servicetype:instanceid
        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(pattern: "services:*");

        if (keys is null || !keys.Any())
            return Array.Empty<ServiceInstance>();

        var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

        if (entries is null || entries.Length == 0)
            return Array.Empty<ServiceInstance>();

        var services = new List<ServiceInstance>();

        for (int i = 0; i < keys.Count(); ++i)
        {
            if (entries.ElementAt(i).HasValue)
            {
                //key pattern: services:servicetype:instanceid
                var keySplit = keys.ElementAt(i).ToString().Split(':');

                string serviceType = keySplit[1];
                Guid instanceId = Guid.Parse(keySplit[2]);

                var addresses = entries.ElementAt(i).ToString().Split(';').ToHashSet();

                services.Add(new ServiceInstance(instanceId, serviceType, addresses));
            }
        }
        return services;
    }

    public async Task<IList<ServiceInstance>> GetServiceInstancesOfType(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        //key pattern: services:servicetype:instanceid
        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(pattern: "services:" + serviceType + ":*");

        if (keys is null || !keys.Any())
            return Array.Empty<ServiceInstance>();

        var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

        if (entries is null || entries.Length == 0)
            return Array.Empty<ServiceInstance>();

        var services = new List<ServiceInstance>();

        for (int i = 0; i < keys.Count(); ++i)
        {
            if (entries.ElementAt(i).HasValue)
            {
                //key pattern: services:servicetype:instanceid
                Guid instanceId = Guid.Parse(keys.ElementAt(i).ToString().Split(':')[2]);
                var addresses = entries.ElementAt(i).ToString().Split(';').ToHashSet();
                services.Add(new ServiceInstance(instanceId, serviceType, addresses));
            }
        }
        return services;
    }

    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInstance serviceInstance)
    {
        if (serviceInstance is null)
            throw new ArgumentNullException(nameof(serviceInstance));

        return _redisDb.KeyExpireAsync(serviceInstance.Key, _options.CurrentValue.Expiry);
    }
}
