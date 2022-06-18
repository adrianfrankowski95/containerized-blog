using StackExchange.Redis;

namespace Blog.Services.Discovery.API.Models;

public struct ServiceId
{
    private readonly string _value;
    private ServiceId(ServiceType type)
    {
        _value = "service:" + type.ToString();
    }
    public static ServiceId FromType(ServiceType type) => new(type);

    public override string ToString() => _value;

    public static implicit operator string(ServiceId id) => id._value;
    public static implicit operator RedisKey(ServiceId id) => id._value;
}