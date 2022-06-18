using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Blog.Services.Discovery.API.Grpc;

public class DiscoveryService : GrpcDiscoveryService.GrpcDiscoveryServiceBase
{
    private readonly ILogger<DiscoveryService> _logger;
    public DiscoveryService(ILogger<DiscoveryService> logger)
    {
        _logger = logger;
    }

    public override Task<GetRegisteredServiceUrlsResponse> GetRegisteredServiceUrls(
        GetRegisteredServiceUrlsRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<GetRegisteredServicesResponse> GetRegisteredServices(Empty request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}
