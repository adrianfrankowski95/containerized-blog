namespace Blog.Services.Discovery.API.Models;

public class ServiceInstance
{
    public ServiceInstanceKey Key { get; }
    public Guid InstanceId => Key.InstanceId;
    public string ServiceType => Key.ServiceType;
    public IReadOnlySet<string> Addresses { get; }

    public ServiceInstance(in ServiceInstanceKey key, IReadOnlySet<string> serviceAddresses)
        : this(key.InstanceId, key.ServiceType, serviceAddresses)
    {

    }

    public ServiceInstance(Guid instanceId, string serviceType, IReadOnlySet<string> serviceAddresses)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (!(serviceAddresses?.Any() ?? false))
            throw new ArgumentNullException(nameof(serviceAddresses));

        Key = new(serviceType, instanceId);
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