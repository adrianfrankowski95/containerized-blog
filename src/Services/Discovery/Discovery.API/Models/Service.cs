namespace Blog.Services.Discovery.API.Models;

public class Service
{
    public Service(ServiceType serviceType, IEnumerable<string> urls)
    {
        Id = ServiceId.FromType(serviceType);
        Type = serviceType;
        Urls = urls;
    }
    public ServiceId Id { get; set; }
    public ServiceType Type { get; set; }
    public IEnumerable<string> Urls { get; set; }
}