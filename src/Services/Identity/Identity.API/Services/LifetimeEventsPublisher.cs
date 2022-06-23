using Blog.Services.Identity.API.Configs;
using Blog.Services.Integration.Events;
using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Services;

public class LifetimeEventsPublisher : BackgroundService
{
    private readonly IBus _bus;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<ServiceInstanceConfig> _config;
    private readonly ILogger<LifetimeEventsPublisher> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public LifetimeEventsPublisher(
        IBus bus,
        IServiceProvider serviceProvider,
        IOptions<ServiceInstanceConfig> config,
        IHostApplicationLifetime lifetime,
        ILogger<LifetimeEventsPublisher> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //We are waiting for an ApplicationStarted token trigger, because only then
        //IServer instance is available and can be resolved by a service provider
        //in order to obtain app's Addresses
        await WaitForStartupOrCancellationAsync(_lifetime, stoppingToken).ConfigureAwait(false);

        if (stoppingToken.IsCancellationRequested)
        {
            var config = _config.Value;
            _logger.LogWarning("----- Cancellation requested for {Type} service instance before events initialization, ID: {Id}",
                config.ServiceType, config.InstanceId);
            return;
        }

        var addresses = await GetAddressesAsync().ConfigureAwait(false);

        using var registration = _lifetime.ApplicationStopping.Register(async () => await PublishStoppedEventAsync(addresses).ConfigureAwait(false));

        await PublishStartedEventAsync(addresses, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_config.Value.HeartbeatInterval, stoppingToken).ConfigureAwait(false);
            await PublishHeartbeatEvent(addresses, stoppingToken).ConfigureAwait(false);
        }
    }

    private Task PublishStartedEventAsync(IEnumerable<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance started: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceStartedEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private Task PublishStoppedEventAsync(IEnumerable<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance stopped: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceStoppedEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private Task PublishHeartbeatEvent(IEnumerable<string> addresses, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} heartbeat: {Id} - {Addresses}", config.ServiceType, config.InstanceId, string.Join("; ", addresses));
        return _bus.Publish<ServiceInstanceHeartbeatEvent>(new(config.InstanceId, config.ServiceType, addresses), stoppingToken);
    }

    private async Task<IEnumerable<string>> GetAddressesAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var server = scope.ServiceProvider.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        if (addressFeature is null || !addressFeature.Addresses.Any())
            throw new InvalidOperationException($"Error getting {_config.Value.ServiceType} addresses");

        return addressFeature.Addresses;
    }

    private static Task WaitForStartupOrCancellationAsync(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var appStartedOrCancelled = new TaskCompletionSource();
        var startedOrCancelledToken = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStarted, stoppingToken).Token;

        using var registration = startedOrCancelledToken.Register(() => appStartedOrCancelled.SetResult());

        return appStartedOrCancelled.Task;
    }
}
