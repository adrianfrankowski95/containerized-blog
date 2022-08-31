namespace Blog.Services.Identity.API.Infrastructure.Services;

public interface IDiscoveryService
{
    public Task<string> GetAddressOfServiceTypeAsync(string serviceType);
}