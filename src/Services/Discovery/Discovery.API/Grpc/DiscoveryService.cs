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

    public override Task<DiscoverServiceResponse> DiscoverService(DiscoverServiceRequest request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }

    public override Task<DiscoverServicesResponse> DiscoverServices(Empty request, ServerCallContext context)
    {
        throw new NotImplementedException();
    }
}
