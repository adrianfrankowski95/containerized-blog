using Blog.Integration.Events;
using Blog.Services.Comments.API.Configs;
using Blog.Services.Comments.API.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace Blog.Services.Comments.API.Services;

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
        await WaitForStartupOrCancellationAsync(_lifetime, stoppingToken, out CancellationTokenRegistration tokenRegistration).ConfigureAwait(false);
        await tokenRegistration.DisposeAsync().ConfigureAwait(false);

        var config = _config.Value;
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("----- Cancellation requested for {Type} service instance before events initialization, ID: {Id}",
                config.ServiceType, config.InstanceId);
            return;
        }

        var address = GetAddress();

        using var registration = _lifetime.ApplicationStopping.Register(async () => await PublishStoppedEventAsync(address).ConfigureAwait(false));

        await PublishStartedEventAsync(address, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(config.HeartbeatInterval, stoppingToken).ConfigureAwait(false);
            await PublishHeartbeatEvent(address, stoppingToken).ConfigureAwait(false);
        }
    }

    private Task PublishStartedEventAsync(string address, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance started: {Id} - {Address}", config.ServiceType, config.InstanceId, address);
        return _bus.Publish<ServiceInstanceStartedIntegrationEvent>(
            new(config.InstanceId, config.ServiceType, new HashSet<string>(new[] { address })),
            stoppingToken);
    }

    private Task PublishStoppedEventAsync(string address, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance stopped: {Id} - {Address}", config.ServiceType, config.InstanceId, address);
        return _bus.Publish<ServiceInstanceStoppedIntegrationEvent>(
            new(config.InstanceId, config.ServiceType, new HashSet<string>(new[] { address })),
            stoppingToken);
    }

    private Task PublishHeartbeatEvent(string address, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} heartbeat: {Id} - {Address}", config.ServiceType, config.InstanceId, address);
        return _bus.Publish<ServiceInstanceHeartbeatIntegrationEvent>(
            new(config.InstanceId, config.ServiceType, new HashSet<string>(new[] { address })),
            stoppingToken);
    }

    private string GetAddress()
    {
        var addressFeature = _server.Features.Get<IServerAddressesFeature>();

        if (addressFeature is null)
            throw new InvalidOperationException("Could not get a server addresses feature.");

        if (addressFeature.Addresses.IsNullOrEmpty())
            throw new InvalidOperationException($"Error getting {_config.Value.ServiceType} Addresses.");

        var cfg = _config.Value;
        return addressFeature.Addresses.First().Split(':')[0] + "://" + cfg.Hostname + ":" + cfg.Port;
    }

    private static Task WaitForStartupOrCancellationAsync(
        IHostApplicationLifetime lifetime,
        CancellationToken stoppingToken,
        out CancellationTokenRegistration registration)
    {
        var appStartedOrCancelled = new TaskCompletionSource();
        var startedOrCancelledToken = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStarted, stoppingToken).Token;

        registration = startedOrCancelledToken.Register(() => appStartedOrCancelled.SetResult());

        return appStartedOrCancelled.Task;
    }
}