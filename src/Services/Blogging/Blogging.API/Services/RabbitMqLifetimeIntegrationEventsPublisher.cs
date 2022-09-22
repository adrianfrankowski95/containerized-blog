using Blog.Services.Blogging.API.Configs;
using Blog.Integration.Events;
using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace Blog.Services.Blogging.API.Services;

public class RabbitMqLifetimeIntegrationEventsPublisher : BackgroundService
{
    private readonly IBus _bus;
    private readonly IServer _server;
    private readonly IOptions<InstanceConfig> _config;
    private readonly ILogger<RabbitMqLifetimeIntegrationEventsPublisher> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public RabbitMqLifetimeIntegrationEventsPublisher(
       IBus bus,
       IServer server,
       IOptions<InstanceConfig> config,
       IHostApplicationLifetime lifetime,
       ILogger<RabbitMqLifetimeIntegrationEventsPublisher> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // We are waiting for an ApplicationStarted token trigger, because only then
        // IServer instance is available and can be resolved by a service provider
        // in order to obtain app's Addresses
        await WaitForStartupOrCancellationAsync(_lifetime, stoppingToken).ConfigureAwait(false);

        var config = _config.Value;
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("----- Cancellation requested for {Type} service instance before events initialization, ID: {Id}",
                config.ServiceType, config.InstanceId);
            return;
        }

        var addresses = GetAddresses();

        using var registration = _lifetime.ApplicationStopping.Register(async () => await PublishStoppedEventAsync(addresses).ConfigureAwait(false));

        await PublishStartedEventAsync(addresses, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(config.HeartbeatInterval, stoppingToken).ConfigureAwait(false);
            await PublishHeartbeatEvent(addresses, stoppingToken).ConfigureAwait(false);
        }
    }

    private Task PublishStartedEventAsync(HashSet<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance started: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceStartedIntegrationEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private Task PublishStoppedEventAsync(HashSet<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance stopped: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceStoppedIntegrationEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private Task PublishHeartbeatEvent(HashSet<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} heartbeat: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceHeartbeatIntegrationEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private HashSet<string> GetAddresses()
    {
        var addressFeature = _server.Features.Get<IServerAddressesFeature>();

        if (addressFeature is null || !addressFeature.Addresses.Any())
            throw new InvalidOperationException($"Error getting {_config.Value.ServiceType} Addresses");

        return addressFeature.Addresses.ToHashSet();
    }

    private static Task WaitForStartupOrCancellationAsync(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var appStartedOrCancelled = new TaskCompletionSource();
        var startedOrCancelledToken = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStarted, stoppingToken).Token;

        using var registration = startedOrCancelledToken.Register(() => appStartedOrCancelled.SetResult());

        return appStartedOrCancelled.Task;
    }
}
