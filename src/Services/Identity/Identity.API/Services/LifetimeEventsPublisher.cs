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
        //in order to obtain app's URLs
        await WaitForStartupOrCancellationAsync(_lifetime, stoppingToken).ConfigureAwait(false);

        if (stoppingToken.IsCancellationRequested)
        {
            var config = _config.Value;
            _logger.LogWarning("----- Cancellation requested for {Type} service instance before events initialization, ID: {Id}",
                config.ServiceType, config.InstanceId);
            return;
        }

        var urls = await GetAppUrlsAsync().ConfigureAwait(false);
        
        using var registration = _lifetime.ApplicationStopping.Register(async () => await PublishStoppedEventAsync(urls).ConfigureAwait(false));

        await PublishStartedEventAsync(urls, stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_config.Value.HeartbeatInterval, stoppingToken).ConfigureAwait(false);
            await PublishHeartbeatEvent(urls, stoppingToken).ConfigureAwait(false);
        }
    }

    private Task PublishStartedEventAsync(IEnumerable<string> appUrls, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance started: {Id} - {Urls}", config.ServiceType, config.InstanceId, string.Join("; ", appUrls));
        return _bus.Publish<ServiceInstanceStartedEvent>(new(config.InstanceId, config.ServiceType, appUrls), stoppingToken);
    }

    private Task PublishStoppedEventAsync(IEnumerable<string> appUrls, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} service instance stopped: {Id} - {Urls}", config.ServiceType, config.InstanceId, string.Join("; ", appUrls));
        return _bus.Publish<ServiceInstanceStoppedEvent>(new(config.InstanceId, config.ServiceType, appUrls), stoppingToken);
    }

    private Task PublishHeartbeatEvent(IEnumerable<string> appUrls, CancellationToken stoppingToken = default)
    {
        var config = _config.Value;

        _logger.LogInformation("----- {Type} heartbeat: {Id} - {Urls}", config.ServiceType, config.InstanceId, string.Join("; ", appUrls));
        return _bus.Publish<ServiceInstanceHeartbeatEvent>(new(config.InstanceId, config.ServiceType, appUrls), stoppingToken);
    }

    private async Task<IEnumerable<string>> GetAppUrlsAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var server = scope.ServiceProvider.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        if (addressFeature is null || !addressFeature.Addresses.Any())
            throw new InvalidOperationException($"Error getting {_config.Value.ServiceType} URLs");

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
