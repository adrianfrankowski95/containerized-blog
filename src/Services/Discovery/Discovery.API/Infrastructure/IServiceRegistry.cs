using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<bool> RegisterServiceInstance(ServiceInstance serviceInstanceData);
    public Task<bool> UnregisterServiceInstance(ServiceInstance serviceInstanceData);
    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInstance serviceInstanceData);
    public Task<IList<ServiceInstance>> GetAllServiceInstances();
    public Task<IList<ServiceInstance>> GetServiceInstancesOfType(string serviceType);

}
