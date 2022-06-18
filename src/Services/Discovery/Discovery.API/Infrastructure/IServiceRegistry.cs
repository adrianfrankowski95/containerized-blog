using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<(long changes, ServiceInfo updatedServiceInfo)> RegisterService(ServiceInfo service);
    public Task<(long changes, ServiceInfo updatedServiceInfo)> UnregisterService(ServiceInfo service);
    public Task<IEnumerable<ServiceInfo>> GetRegisteredServicesAsync();
    public Task<IEnumerable<string>> GetRegisteredServiceUrlsAsync(ServiceType serviceType);
}
