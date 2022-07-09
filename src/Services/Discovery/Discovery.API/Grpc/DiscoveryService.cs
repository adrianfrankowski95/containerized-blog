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

    public override async Task<GetServiceInstancesDataOfTypeResponse> GetServiceInstancesDataOfType(GetServiceInstancesDataOfTypeRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ServiceType))
            throw new InvalidOperationException($"{request.ServiceType} must not be null");

        _logger.LogInformation("----- Handling Grpc Get Service Instances Data Of Type request for {ServiceType}", request.ServiceType);

        var instancesData = await _serviceRegistry.GetServiceInstancesDataOfType(request.ServiceType).ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following {ServiceType} data from registry: {Data}",
            request.ServiceType, string.Join("; ", instancesData.Select(x => $"instance ID: {x.InstanceId}, addresses: {string.Join("; ", x.Addresses)}")));

        return new GetServiceInstancesDataOfTypeResponse
        {
            Data = { Array.ConvertAll(instancesData.ToArray(), x => new ServiceInstanceData
            {
                InstanceId = x.InstanceId.ToString(),
                ServiceType = x.ServiceType,
                Addresses = { x.Addresses }
            })}
        };
    }

    public override async Task<GetAllServiceInstancesDataResponse> GetAllServiceInstancesData(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("----- Handling Grpc Get All Service Instances Data request");

        var instancesData = await _serviceRegistry.GetAllServiceInstancesData().ConfigureAwait(false);

        _logger.LogInformation("----- Successfully fetched following service instances data: {Data}",
            string.Join("; ", instancesData.Select(x => $"service type: {x.ServiceType}, instance ID: {x.InstanceId}, addresses: {string.Join("; ", x.Addresses)}")));

        return new GetAllServiceInstancesDataResponse
        {
            Data = { Array.ConvertAll(instancesData.ToArray(), x => new ServiceInstanceData
            {
                InstanceId = x.InstanceId.ToString(),
                ServiceType = x.ServiceType,
                Addresses = { x.Addresses }
            })}
        };
    }
}
