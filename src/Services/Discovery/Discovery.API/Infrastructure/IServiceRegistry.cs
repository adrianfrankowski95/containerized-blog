using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<bool> RegisterServiceInstance(ServiceInstanceData serviceInstanceData);
    public Task<bool> UnregisterServiceInstance(ServiceInstanceData serviceInstanceData);
    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInstanceData serviceInstanceData);
    public Task<IList<ServiceInstanceData>> GetAllServiceInstancesData();
    public Task<IList<ServiceInstanceData>> GetServiceInstancesDataOfType(string serviceType);

}
