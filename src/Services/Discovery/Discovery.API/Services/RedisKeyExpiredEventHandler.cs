using Blog.Integration.Events;
using Blog.Services.Discovery.API.Models;
using MassTransit;
using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Services;

public class RedisKeyExpiredEventHandler : IHostedService
{
    private readonly IBus _bus;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisKeyExpiredEventHandler> _logger;

    public RedisKeyExpiredEventHandler(IBus bus, IConnectionMultiplexer redis, ILogger<RedisKeyExpiredEventHandler> logger)
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
            _logger.LogInformation("----- Received key expired event from Redis, channel: {Channel}, message: {Message}.", channel, message);

            var key = ServiceInstanceKey.FromString(message.ToString());

            _logger.LogInformation("----- Extracted following key: {Key}.", key);
            _logger.LogInformation("----- Publishing service instance unregistered event, instance ID: {InstanceId}, service type: {ServiceType}.",
                key.InstanceId, key.ServiceType);

            await _bus.Publish(new ServiceInstanceUnregisteredIntegrationEvent(key.InstanceId, key.ServiceType)).ConfigureAwait(false);
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _redis.GetSubscriber(_redis.GetServer(_redis.GetEndPoints().Single())).UnsubscribeAsync("__keyevent@0__:expired").ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
