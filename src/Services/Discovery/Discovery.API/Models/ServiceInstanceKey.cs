namespace Blog.Services.Discovery.API.Models;

public readonly struct ServiceInstanceKey
{
    public const string Prefix = "services";
    public string ServiceType { get; }
    public Guid InstanceId { get; }

    public ServiceInstanceKey(string serviceType, Guid instanceId)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        if (instanceId.Equals(Guid.Empty))
            throw new ArgumentException("Instance ID must not be empty");

        ServiceType = serviceType;
        InstanceId = instanceId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not ServiceInstanceKey key)
            return false;

        if (ReferenceEquals(this, key))
            return true;

        return string.Equals(ServiceType, key.ServiceType, StringComparison.Ordinal) && InstanceId.Equals(key.InstanceId);
    }

    public override int GetHashCode() => ServiceType.GetHashCode();

    public override string ToString() =>
        string.IsNullOrWhiteSpace(ServiceType) || InstanceId.Equals(Guid.Empty)
            ? throw new InvalidDataException("Service instance key must not be null or empty.")
            : Prefix + ":" + ServiceType + ":" + InstanceId;

    public static ServiceInstanceKey FromString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var keySplit = key.Split(':');

        if (keySplit.Length != 3
            || !string.Equals(keySplit[0], Prefix, StringComparison.Ordinal)
            || !Guid.TryParse(keySplit[2], out Guid instanceId))
        {
            throw new FormatException("Invalid service instance key format.");
        }

        var serviceType = keySplit[1];

        return new ServiceInstanceKey(serviceType, instanceId);
    }
}