using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure.Repositories;

public interface IServiceRepository
{
    public Task<long> AddServiceAsync(Service service);
    public Task<long> RemoveServiceAsync(Service service);
    public IAsyncEnumerable<Service> GetServicesAsync();
    public IAsyncEnumerable<string> GetServiceUrlsAsync(ServiceType serviceType);
}
