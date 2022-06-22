namespace Blog.Services.Discovery.API.Models;

public class ServiceInfo
{
    public ServiceInfo(Guid instanceId, string serviceType, IEnumerable<string> urls)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (urls is null || !urls.Any())
            throw new ArgumentNullException(nameof(urls));

        InstanceId = instanceId;
        ServiceType = serviceType;
        Urls = urls.ToHashSet();
    }
    public string Key => "services:" + ServiceType + ":" + InstanceId;
    public Guid InstanceId { get; }
    public string ServiceType { get; }
    public HashSet<string> Urls { get; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not ServiceInfo serviceInfo)
            return false;

        if (ReferenceEquals(this, serviceInfo))
            return true;

        bool equalIds = InstanceId.Equals(serviceInfo.InstanceId);
        bool equalType = string.Equals(ServiceType, serviceInfo.ServiceType, StringComparison.OrdinalIgnoreCase);
        bool equalUrls = Urls.SetEquals(serviceInfo.Urls);

        return equalIds && equalType && equalUrls;
    }

    public override int GetHashCode()
        => InstanceId.GetHashCode();
}