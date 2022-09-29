using System.Collections.Immutable;
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

    public async Task<IReadOnlySet<Models.ServiceInstance>> GetInstancesOfTypeAsync(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        _logger.LogInformation("----- Sending grpc request get service type instances info to discovery service, serviceType: {ServiceType}.", serviceType);

        var response = await _client.GetServiceInstancesOfTypeAsync(
            new GetServiceInstancesOfTypeRequest { ServiceType = serviceType })
            .ConfigureAwait(false);

        if (!(response?.ServiceInstances?.Any() ?? false))
            throw new InvalidDataException($"Error retrieving service instances of type {serviceType} from Discovery Grpc service.");

        _logger.LogInformation("----- Received grpc request get service type instances info response: {Response}.",
            string.Join("; ", response.ServiceInstances.Select(x => $"{x.ServiceType} - {x.InstanceId}: {string.Join(", ", x.Addresses)}.")));

        return response?.ServiceInstances?.Select(x =>
            new Models.ServiceInstance(Guid.Parse(x.InstanceId), x.Addresses.ToHashSet())).ToImmutableHashSet()
            ?? ImmutableHashSet<Models.ServiceInstance>.Empty;
    }

    public async Task<IReadOnlyDictionary<string, HashSet<Models.ServiceInstance>>> GetServicesAsync()
    {
        _logger.LogInformation("----- Sending grpc request get all service instances info to discovery service.");

        var response = await _client.GetAllServiceInstancesAsync(new Google.Protobuf.WellKnownTypes.Empty())
            .ConfigureAwait(false);

        if (!(response?.ServiceInstances?.Any() ?? false))
            throw new InvalidDataException($"Error retrieving all service instances from Discovery Grpc service.");

        _logger.LogInformation("----- Received grpc request get all service instances info response: {Response}.",
            string.Join("; ", response.ServiceInstances.Select(x => $"{x.ServiceType} - {x.InstanceId}: {string.Join(", ", x.Addresses)}")));

        var result = new Dictionary<string, HashSet<Models.ServiceInstance>>();

        foreach (var serviceInstance in response?.ServiceInstances ?? Enumerable.Empty<ServiceInstance>())
        {
            var instance = new Models.ServiceInstance(Guid.Parse(serviceInstance.InstanceId), serviceInstance.Addresses.ToHashSet());

            if (!result.TryAdd(serviceInstance.ServiceType, new HashSet<Models.ServiceInstance>(new[] { instance })))
                result[serviceInstance.ServiceType].Add(instance);
        }

        return result.ToImmutableDictionary();
    }
}