using Blog.Gateways.WebGateway.API.Models;
using Blog.Services.Discovery.API.Grpc;

namespace Blog.Gateways.WebGateway.API.Services;

public class DiscoveryService : IDiscoveryService
{
    private readonly GrpcDiscoveryService.GrpcDiscoveryServiceClient _client;

    public DiscoveryService(GrpcDiscoveryService.GrpcDiscoveryServiceClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<HashSet<ServiceInstanceInfo>> GetAddressesOfServiceTypeAsync(string serviceType)
    {
        var response = await _client.GetServiceInstancesDataOfTypeAsync(
            new GetServiceInstancesDataOfTypeRequest { ServiceType = serviceType })
            .ConfigureAwait(false);

        return response.Data.Select(x =>
            new ServiceInstanceInfo(Guid.Parse(x.InstanceId), x.Addresses.ToHashSet())).ToHashSet();
    }

    public async Task<IDictionary<string, HashSet<ServiceInstanceInfo>>> GetAllAddressesAsync()
    {
        var response = await _client.GetAllServiceInstancesDataAsync(new Google.Protobuf.WellKnownTypes.Empty())
            .ConfigureAwait(false);

        var result = new Dictionary<string, HashSet<ServiceInstanceInfo>>();

        foreach (var instanceData in response.Data)
        {
            var input = new ServiceInstanceInfo(Guid.Parse(instanceData.InstanceId), instanceData.Addresses.ToHashSet());

            if (!result.TryAdd(instanceData.ServiceType, new HashSet<ServiceInstanceInfo>(new[] { input })))
                result[instanceData.ServiceType].Add(input);
        }

        return result;
    }
}