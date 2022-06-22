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

    public override Task<GetUrlsOfServiceResponse> GetUrlsOfService(GetUrlsOfServiceRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<GetUrlsResponse> GetUrls(Empty request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}
