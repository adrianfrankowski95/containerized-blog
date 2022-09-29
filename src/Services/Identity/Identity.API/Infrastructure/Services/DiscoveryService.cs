using Blog.Services.Discovery.API.Grpc;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class DiscoveryService : IDiscoveryService
{
    private readonly GrpcDiscoveryService.GrpcDiscoveryServiceClient _client;
    private readonly ILogger<DiscoveryService> _logger;

    public DiscoveryService(GrpcDiscoveryService.GrpcDiscoveryServiceClient client, ILogger<DiscoveryService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetAddressOfServiceTypeAsync(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        _logger.LogInformation("----- Sending grpc request Get Address of Service Type to discovery service, serviceType: {ServiceType}", serviceType);

        var response = await _client.GetAddressOfServiceTypeAsync(
            new GetAddressOfServiceTypeRequest { ServiceType = serviceType })
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response?.Address))
            throw new InvalidDataException($"Error retrieving service address of type {serviceType} from Discovery Grpc service");

        _logger.LogInformation("----- Received grpc request Get Address of Service Type response: {Response}", response.Address);

        return response.Address;
    }
}