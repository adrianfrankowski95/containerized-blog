using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<bool> RegisterServiceInstance(ServiceInstance serviceInstance);
    public Task<bool> UnregisterServiceInstance(ServiceInstance serviceInstance);
    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInstance serviceInstance);
    public Task<IList<ServiceInstance>> GetAllServiceInstances();
    public Task<IList<ServiceInstance>> GetServiceInstancesOfType(string serviceType);

}
