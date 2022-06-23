namespace Blog.Services.Discovery.API.Models;

public class ServiceInfo
{
    public ServiceInfo(Guid instanceId, string serviceType, IEnumerable<string> ServiceAddresses)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (Addresses is null || !Addresses.Any())
            throw new ArgumentNullException(nameof(Addresses));

        InstanceId = instanceId;
        ServiceType = serviceType;
        Addresses = Addresses.ToHashSet();
    }
    public string Key => "services:" + ServiceType + ":" + InstanceId;
    public Guid InstanceId { get; }
    public string ServiceType { get; }
    public HashSet<string> Addresses { get; }

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
        bool equalAddresses = Addresses.SetEquals(serviceInfo.Addresses);

        return equalIds && equalType && equalAddresses;
    }

    public override int GetHashCode()
        => InstanceId.GetHashCode();
}