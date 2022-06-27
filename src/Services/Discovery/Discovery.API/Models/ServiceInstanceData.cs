namespace Blog.Services.Discovery.API.Models;

public class ServiceInstanceData
{
    public ServiceInstanceData(Guid instanceId, string serviceType, HashSet<string> serviceAddresses)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (serviceAddresses is null || !serviceAddresses.Any())
            throw new ArgumentNullException(nameof(serviceAddresses));

        InstanceId = instanceId;
        ServiceType = serviceType;
        Addresses = serviceAddresses;
    }

    public string Key => "services:" + ServiceType + ":" + InstanceId;
    public Guid InstanceId { get; }
    public string ServiceType { get; }
    public HashSet<string> Addresses { get; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not ServiceInstanceData serviceInstanceData)
            return false;

        if (ReferenceEquals(this, serviceInstanceData))
            return true;

        bool equalIds = InstanceId.Equals(serviceInstanceData.InstanceId);
        bool equalType = string.Equals(ServiceType, serviceInstanceData.ServiceType, StringComparison.OrdinalIgnoreCase);
        bool equalAddresses = Addresses.SetEquals(serviceInstanceData.Addresses);

        return equalIds && equalType && equalAddresses;
    }

    public override int GetHashCode()
        => InstanceId.GetHashCode();
}