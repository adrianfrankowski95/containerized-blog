using Blog.Services.Integration.Events;
using MassTransit;
using StackExchange.Redis;

namespace Discovery.API.Services;

public class ServiceRegistryKeyExpirationNotifier : IHostedService
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _redis.GetSubscriber(_redis.GetServer(_redis.GetEndPoints().Single())).SubscribeAsync("__keyevent@0__:expired", async (channel, message) =>
        {
            _logger.LogInformation("----- Received key expired event from Redis, channel: {Channel}, message: {Message}", channel, message);

            var key = message.ToString();

            _logger.LogInformation("----- Extracted following key: {Key}", key);

            //key pattern: services:servicetype:instanceid     
            var keySplit = key.Split(':');
            var serviceType = keySplit[1];
            var instanceId = Guid.Parse(keySplit[2]);

            _logger.LogInformation("----- Publishing service instance unregistered event, instance ID: {InstanceId}, service type: {ServiceType}",
                instanceId, serviceType);

            await _bus.Publish(new ServiceInstanceUnregisteredEvent(instanceId, serviceType)).ConfigureAwait(false);
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _redis.GetSubscriber(_redis.GetServer(_redis.GetEndPoints().Single())).UnsubscribeAsync("__keyevent@0__:expired");
    }
}
