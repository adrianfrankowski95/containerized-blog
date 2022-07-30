using Blog.Gateways.WebGateway.API.Models;

namespace Blog.Gateways.WebGateway.API.Services;

public interface IDiscoveryService
{
    public Task<HashSet<ServiceInstance>> GetServiceInstancesOfTypeAsync(string serviceType);
    public Task<IDictionary<string, HashSet<ServiceInstance>>> GetAllInstancesAsync();
}