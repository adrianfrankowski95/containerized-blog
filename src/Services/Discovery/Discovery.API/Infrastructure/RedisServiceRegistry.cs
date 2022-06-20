using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
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

    public Task<bool> RegisterServiceInstance(ServiceInfo serviceInfo)
    {
        if (serviceInfo is null)
            throw new ArgumentNullException(nameof(serviceInfo));

        return _redisDb.StringSetAsync(serviceInfo.Key, string.Join(';', serviceInfo.Urls), _options.CurrentValue.Expiry);
    }

    public Task<bool> UnregisterServiceInstance(ServiceInfo serviceInfo)
    {
        if (serviceInfo is null)
            throw new ArgumentNullException(nameof(serviceInfo));

        return _redisDb.KeyDeleteAsync(serviceInfo.Key);
    }

    public async Task<IDictionary<string, HashSet<string>>> GetRegisteredServicesAsync()
    {
        //key pattern: services:servicetype:instanceid
        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(pattern: "services:*");

        if (keys is not null && keys.Any())
        {
            var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

            if (entries is not null && entries.Length > 0)
            {
                var services = new Dictionary<string, HashSet<string>>();

                for (int i = 0; i < keys.Count(); ++i)
                {
                    if (entries.ElementAt(i).HasValue)
                    {
                        //key pattern: services:servicetype:instanceid
                        string serviceType = keys.ElementAt(i).ToString().Split(':')[1];

                        var urls = entries.ElementAt(i).ToString().Split(';').ToHashSet();

                        if (!services.TryAdd(serviceType, urls))
                            services[serviceType].UnionWith(urls);
                    }
                }

                return services;
            }

        }

        return ImmutableDictionary<string, HashSet<string>>.Empty;
    }

    public async Task<IEnumerable<string>> GetRegisteredServiceTypeUrlsAsync(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        //key pattern: services:servicetype:instanceid
        var keys = _redis.GetServer(_redis.GetEndPoints().Single()).Keys(pattern: "services:" + serviceType + ":*");

        if (keys is not null && keys.Any())
        {
            var entries = await _redisDb.StringGetAsync(keys.ToArray()).ConfigureAwait(false);

            if (entries is not null && entries.Length > 0)
                return entries.SelectMany(x => x.ToString().Split(';'));
        }

        return Enumerable.Empty<string>();
    }

    public async Task<bool> ServiceInstanceExistsAsync(ServiceInfo serviceInfo, bool refreshExpiration = false)
    {
        if (serviceInfo is null)
            throw new ArgumentNullException(nameof(serviceInfo));

        bool exists = await _redisDb.KeyExistsAsync(serviceInfo.Key).ConfigureAwait(false);

        if (exists && refreshExpiration)
            await _redisDb.KeyExpireAsync(serviceInfo.Key, _options.CurrentValue.Expiry).ConfigureAwait(false);

        return exists;
    }
}
