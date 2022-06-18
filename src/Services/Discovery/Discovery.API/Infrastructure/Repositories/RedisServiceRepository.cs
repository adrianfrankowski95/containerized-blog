using System.Text.Json;
using Blog.Services.Discovery.API.Models;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Infrastructure.Repositories;

public class RedisServiceRepository : IServiceRepository
{
    private readonly IConnectionMultiplexer _redis;

    public RedisServiceRepository(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public Task<long> AddServiceAsync(Service service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var db = _redis.GetDatabase();

        return db.SetAddAsync(service.Id, service.Urls.Select(x => (RedisValue)x).ToArray());
    }

    public Task<long> RemoveServiceAsync(Service service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var db = _redis.GetDatabase();

        return db.SetRemoveAsync(service.Id, service.Urls.Cast<RedisValue>().ToArray());
    }

    public async IAsyncEnumerable<Service> GetServicesAsync()
    {
        var db = _redis.GetDatabase();

        var serviceTypes = Enum.GetNames<ServiceType>();

        foreach (var type in serviceTypes)
        {
            var result = await db.StringGetAsync(type).ConfigureAwait(false);

            if (result.HasValue)
            {
                var urls = JsonSerializer.Deserialize<string[]>(result!);
                if (urls is null)
                    throw new InvalidDataException("Cannot deserialize RedisValue to Urls array");

                yield return new Service(Enum.Parse<ServiceType>(type), urls);
            }
        }
    }

    public async IAsyncEnumerable<string> GetServiceUrlsAsync(ServiceType serviceType)
    {
        var db = _redis.GetDatabase();

        var result = await db.StringGetAsync(ServiceId.FromType(serviceType)).ConfigureAwait(false);

        if (result.HasValue)
        {
            var urls = JsonSerializer.Deserialize<string[]>(result!);
            if (urls is null)
                throw new InvalidDataException("Cannot deserialize RedisValue to Urls array");

            foreach (var url in urls)
            {
                yield return url;
            }
        }
    }
}
