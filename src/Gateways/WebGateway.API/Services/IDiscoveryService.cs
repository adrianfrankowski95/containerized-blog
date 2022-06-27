using Blog.Gateways.WebGateway.API.Models;

namespace Blog.Gateways.WebGateway.API.Services;

public interface IDiscoveryService
{
    public Task<HashSet<ServiceInstanceInfo>> GetAddressesOfServiceTypeAsync(string serviceType);
    public Task<IDictionary<string, HashSet<ServiceInstanceInfo>>> GetAllAddressesAsync();
}