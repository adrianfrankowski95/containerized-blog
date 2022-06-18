namespace Blog.Services.Discovery.API.Models;

public class ServiceInfo
{
    public ServiceInfo(ServiceType serviceType, IEnumerable<string> urls)
    {
        Id = ServiceId.FromType(serviceType);
        Type = serviceType;
        Urls = urls;
    }
    public ServiceId Id { get; }
    public ServiceType Type { get; }
    public IEnumerable<string> Urls { get; set; }
}