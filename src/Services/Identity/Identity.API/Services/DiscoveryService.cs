using Blog.Services.Discovery.API.Grpc;
using Blog.Services.Identity.API.Services;

namespace Blog.Gateways.WebGateway.API.Services;

public class DiscoveryService : IDiscoveryService
{
    private readonly GrpcDiscoveryService.GrpcDiscoveryServiceClient _client;
    private readonly ILogger<DiscoveryService> _logger;

    public DiscoveryService(GrpcDiscoveryService.GrpcDiscoveryServiceClient client, ILogger<DiscoveryService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetAddressOfServiceTypeAsync(string serviceType);
    {
        _logger.LogInformation("----- Sending grpc request get service type instances info to discovery service, serviceType: {ServiceType}", serviceType);

        var response = await _client.GetServiceInstancesOfTypeAsync(
            new GetServiceInstancesOfTypeRequest { ServiceType = serviceType })
            .ConfigureAwait(false);

        _logger.LogInformation("----- Received grpc request get service type instances info response: {Response}",
            string.Join("; ", response.ServiceInstances.Select(x => $"{x.ServiceType} - {x.InstanceId}: {string.Join(", ", x.Addresses)}")));

        return response.ServiceInstances.Select(x =>
            new Models.ServiceInstance(Guid.Parse(x.InstanceId), x.Addresses.ToHashSet())).ToHashSet();
    }
}