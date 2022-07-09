using Blog.Gateways.WebGateway.API.Models;
using Blog.Services.Discovery.API.Grpc;

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

    public async Task<HashSet<ServiceInstanceInfo>> GetServiceTypeInstancesInfoAsync(string serviceType)
    {
        _logger.LogInformation("----- Sending grpc request get service type instances info to discovery service, serviceType: {ServiceType}", serviceType);

        var response = await _client.GetServiceInstancesDataOfTypeAsync(
            new GetServiceInstancesDataOfTypeRequest { ServiceType = serviceType })
            .ConfigureAwait(false);

        _logger.LogInformation("----- Received grpc request get service type instances info response: {Response}",
            string.Join("; ", response.Data.Select(x => $"{x.ServiceType} - {x.InstanceId}: {string.Join(", ", x.Addresses)}")));

        return response.Data.Select(x =>
            new ServiceInstanceInfo(Guid.Parse(x.InstanceId), x.Addresses.ToHashSet())).ToHashSet();
    }

    public async Task<IDictionary<string, HashSet<ServiceInstanceInfo>>> GetAllInstancesInfoAsync()
    {
        _logger.LogInformation("----- Sending grpc request get all service instances info to discovery service");

        var response = await _client.GetAllServiceInstancesDataAsync(new Google.Protobuf.WellKnownTypes.Empty())
            .ConfigureAwait(false);

        _logger.LogInformation("----- Received grpc request get all service instances info response: {Response}",
            string.Join("; ", response.Data.Select(x => $"{x.ServiceType} - {x.InstanceId}: {string.Join(", ", x.Addresses)}")));

        var result = new Dictionary<string, HashSet<ServiceInstanceInfo>>();

        foreach (var instanceData in response.Data)
        {
            var instanceInfo = new ServiceInstanceInfo(Guid.Parse(instanceData.InstanceId), instanceData.Addresses.ToHashSet());

            if (!result.TryAdd(instanceData.ServiceType, new HashSet<ServiceInstanceInfo>(new[] { instanceInfo })))
                result[instanceData.ServiceType].Add(instanceInfo);
        }

        return result;
    }
}