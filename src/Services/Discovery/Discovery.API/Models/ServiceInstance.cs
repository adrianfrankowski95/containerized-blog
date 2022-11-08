namespace Blog.Services.Discovery.API.Models;

public class ServiceInstance
{
    public string Key => "services:" + ServiceType + ":" + InstanceId;
    public Guid InstanceId { get; }
    public string ServiceType { get; }
    public IReadOnlySet<string> Addresses { get; }

    public ServiceInstance(Guid instanceId, string serviceType, IReadOnlySet<string> serviceAddresses)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (serviceAddresses is null || !serviceAddresses.Any())
            throw new ArgumentNullException(nameof(serviceAddresses));

        InstanceId = instanceId;
        ServiceType = serviceType;
        Addresses = serviceAddresses;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not ServiceInstance serviceInstanceData)
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