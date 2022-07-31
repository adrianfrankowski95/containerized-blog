namespace Blog.Services.Identity.API.Services;

public interface IDiscoveryService
{
    public Task<string> GetAddressOfServiceTypeAsync(string serviceType);
}