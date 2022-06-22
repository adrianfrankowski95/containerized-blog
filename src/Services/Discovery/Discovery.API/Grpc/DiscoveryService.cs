using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Blog.Services.Discovery.API.Infrastructure;

namespace Blog.Services.Discovery.API.Grpc;

public class DiscoveryService : GrpcDiscoveryService.GrpcDiscoveryServiceBase
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<DiscoveryService> _logger;
    public DiscoveryService(IServiceRegistry serviceRegistry, ILogger<DiscoveryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
    }

    public override async Task<GetUrlsOfServiceResponse> GetUrlsOfService(GetUrlsOfServiceRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ServiceType))
            throw new ArgumentNullException(nameof(request.ServiceType));

        _logger.LogInformation("----- Handling Grpc get urls of service request for {ServiceType}", request.ServiceType);

        var urls = await _serviceRegistry.GetUrlsOfServiceAsync(request.ServiceType).ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following {ServiceType} URLs: {Urls}", request.ServiceType, string.Join("; ", urls));

        return new GetUrlsOfServiceResponse { Urls = { urls } };
    }

    public override async Task<GetUrlsResponse> GetUrls(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("----- Handling Grpc get urls request");

        var servicesUrls = await _serviceRegistry.GetUrlsAsync().ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following URLs: {ServiceUrls}",
            servicesUrls.Select((serviceUrls) => $"{serviceUrls.Key}: {string.Join("; ", serviceUrls.Value)}"));

        return new GetUrlsResponse
        {
            ServiceUrls = { servicesUrls.ToDictionary(k => k.Key, k => new UrlList { Urls = { k.Value } }) }
        };
    }
}
