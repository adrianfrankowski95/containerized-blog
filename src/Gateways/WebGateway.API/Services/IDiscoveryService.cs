using Blog.Gateways.WebGateway.API.Models;

namespace Blog.Gateways.WebGateway.API.Services;

public interface IDiscoveryService
{
    public Task<IReadOnlySet<ServiceInstance>> GetInstancesOfTypeAsync(string serviceType);
    public Task<IReadOnlyDictionary<string, HashSet<ServiceInstance>>> GetServicesAsync();
}