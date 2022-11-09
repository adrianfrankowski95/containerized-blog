namespace Blog.Services.Discovery.API.Models;

readonly public struct ServiceInstanceKey
{
    private const string Prefix = "services";
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

    public bool IsEmpty() => string.IsNullOrWhiteSpace(ServiceType) || InstanceId.Equals(Guid.Empty);

    public override int GetHashCode() => InstanceId.GetHashCode();

    public override string ToString() => IsEmpty()
            ? throw new InvalidDataException("Service instance key must not be empty.")
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

    public static string GetServiceTypeKeyPattern(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
            throw new ArgumentNullException(nameof(serviceType));

        return Prefix + ":" + serviceType + ":*";
    }

    public static string GetAllInstancesKeyPattern() => Prefix;
}