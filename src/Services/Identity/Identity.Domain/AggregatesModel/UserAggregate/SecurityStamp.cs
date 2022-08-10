using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class SecurityStamp : ValueObject<SecurityStamp>
{
    private readonly Guid _value;
    public Instant IssuedAt { get; }

    private SecurityStamp(Guid value)
    {
        _value = value;
        IssuedAt = SystemClock.Instance.GetCurrentInstant();
    }

    //public Guid ToGuid() => _value;

    public static SecurityStamp NewStamp() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
