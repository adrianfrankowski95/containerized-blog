using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<bool> RegisterServiceInstance(ServiceInfo serviceInfo);
    public Task<bool> UnregisterServiceInstance(ServiceInfo serviceInfo);
    public Task<IDictionary<string, HashSet<string>>> GetRegisteredServicesAsync();
    public Task<IEnumerable<string>> GetRegisteredServiceTypeUrlsAsync(string serviceType);
    public Task<bool> ServiceInstanceExistsAsync(ServiceInfo serviceInfo, bool refreshExpiration = false);
}
