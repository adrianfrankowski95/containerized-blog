using Blog.Services.Integration.Events;
using MassTransit;
using StackExchange.Redis;

namespace Discovery.API.Services;

public class ServiceRegistryKeyExpirationNotifier : BackgroundService
{
    private readonly IBus _bus;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ServiceRegistryKeyExpirationNotifier> _logger;

    public ServiceRegistryKeyExpirationNotifier(IBus bus, IConnectionMultiplexer redis, ILogger<ServiceRegistryKeyExpirationNotifier> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(redis));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        return _redis.GetSubscriber().SubscribeAsync("__keyevent@0__:expired", async (channel, message) =>
        {
            _logger.LogInformation("----- Received key expired event from Redis, channel: {Channel}, message: {Message}", channel, message);

            var key = GetKey(channel);

            _logger.LogInformation("----- Extracted following key from channel: {Key}", key);

            //key pattern: services:servicetype:instanceid     
            var keySplit = key.Split(':');
            var instanceId = Guid.Parse(keySplit[2]);
            var serviceType = keySplit[1];

            _logger.LogInformation("----- Publishing service instance unregistered event, instance ID: {InstanceId}, service type: {ServiceType}",
                instanceId, serviceType);

            await _bus.Publish(new ServiceInstanceUnregisteredEvent(instanceId, serviceType)).ConfigureAwait(false);
        });
    }

    private static string GetKey(RedisChannel channel)
    {
        var channelString = channel.ToString();

        var index = channelString.IndexOf(':');
        if (index >= 0 && index < channelString.Length - 1)
            return channelString[(index + 1)..];

        throw new InvalidOperationException($"Could not read a key from channel: {channelString}");
    }
}
