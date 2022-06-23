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

    public override async Task<GetAddressesOfServiceResponse> GetAddressesOfService(GetAddressesOfServiceRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ServiceType))
            throw new ArgumentNullException(nameof(request.ServiceType));

        _logger.LogInformation("----- Handling Grpc get addresses of service request for {ServiceType}", request.ServiceType);

        var Addresses = await _serviceRegistry.GetAddressesOfServiceAsync(request.ServiceType).ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following {ServiceType} addresses: {Addresses}", request.ServiceType, string.Join("; ", Addresses));

        return new GetAddressesOfServiceResponse { Addresses = { Addresses } };
    }

    public override async Task<GetAddressesResponse> GetAddresses(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("----- Handling Grpc get addresses request");

        var servicesAddresses = await _serviceRegistry.GetAddressesAsync().ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following addresses: {ServiceAddresses}",
            servicesAddresses.Select((serviceAddresses) => $"{serviceAddresses.Key}: {string.Join("; ", serviceAddresses.Value)}"));

        return new GetAddressesResponse
        {
            ServiceAddresses = { servicesAddresses.ToDictionary(k => k.Key, k => new AddressList { Addresses = { k.Value } }) }
        };
    }
}
