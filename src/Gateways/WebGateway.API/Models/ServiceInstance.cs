namespace Blog.Gateways.WebGateway.API.Models;

public class ServiceInstance
{
    public ServiceInstance(Guid instanceId, HashSet<string> serviceAddresses)
    {
        if (serviceAddresses is null || !serviceAddresses.Any())
            throw new ArgumentNullException(nameof(serviceAddresses));

        InstanceId = instanceId;
        Addresses = serviceAddresses;
    }
    public Guid InstanceId { get; }
    public HashSet<string> Addresses { get; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not ServiceInstance serviceInstanceInfo)
            return false;

        if (ReferenceEquals(this, serviceInstanceInfo))
            return true;

        bool equalIds = InstanceId.Equals(serviceInstanceInfo.InstanceId);
        bool equalAddresses = Addresses.SetEquals(serviceInstanceInfo.Addresses);

        return equalIds && equalAddresses;
    }

    public override int GetHashCode()
        => InstanceId.GetHashCode();
}