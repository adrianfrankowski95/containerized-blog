using Blog.Services.Discovery.API.Models;

namespace Blog.Services.Discovery.API.Infrastructure;

public interface IServiceRegistry
{
    public Task<bool> RegisterServiceInstance(ServiceInfo serviceInfo);
    public Task<bool> UnregisterServiceInstance(ServiceInfo serviceInfo);
    public Task<bool> TryRefreshServiceInstanceExpiry(ServiceInfo serviceInfo);
    public Task<IDictionary<string, HashSet<string>>> GetUrlsAsync();
    public Task<IEnumerable<string>> GetUrlsOfServiceAsync(string serviceType);

}
