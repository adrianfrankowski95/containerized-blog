using Blog.Services.Discovery.API.Infrastructure;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

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

    public override async Task<GetServiceInstancesOfTypeResponse> GetServiceInstancesOfType(GetServiceInstancesOfTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ServiceType))
            throw new InvalidOperationException($"{request.ServiceType} must not be null");

        _logger.LogInformation("----- Handling Grpc Get Service Instances Data Of Type request for {ServiceType}", request.ServiceType);

        var serviceInstances = await _serviceRegistry.GetServiceInstancesOfType(request.ServiceType).ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following {ServiceType} data from registry: {Data}",
            request.ServiceType, string.Join("; ", serviceInstances.Select(x => $"instance ID: {x.InstanceId}, addresses: {string.Join("; ", x.Addresses)}")));

        return new GetServiceInstancesOfTypeResponse
        {
            ServiceInstances = { Array.ConvertAll(serviceInstances.ToArray(), x => new ServiceInstance
            {
                InstanceId = x.InstanceId.ToString(),
                ServiceType = x.ServiceType,
                Addresses = { x.Addresses }
            })}
        };
    }

    public override async Task<GetAllServiceInstancesResponse> GetAllServiceInstances(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("----- Handling Grpc Get All Service Instances Data request");

        var serviceInstances = await _serviceRegistry.GetAllServiceInstances().ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following service instances data: {Data}",
            string.Join("; ", serviceInstances.Select(x => $"service type: {x.ServiceType}, instance ID: {x.InstanceId}, addresses: {string.Join("; ", x.Addresses)}")));

        return new GetAllServiceInstancesResponse
        {
            ServiceInstances = { Array.ConvertAll(serviceInstances.ToArray(), x => new ServiceInstance
            {
                InstanceId = x.InstanceId.ToString(),
                ServiceType = x.ServiceType,
                Addresses = { x.Addresses }
            })}
        };
    }
}
