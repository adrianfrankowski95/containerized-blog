using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class SecurityStamp : ValueObject<SecurityStamp>
{
    private readonly Guid _value;
    public Instant IssuedAt { get; }

    private SecurityStamp(Guid value, Instant now)
    {
        if (value.Equals(Guid.Empty))
            throw new ArgumentException("Security stamp must not be empty.");

        _value = value;
        IssuedAt = now;
    }

    public override string ToString() => _value.ToString();

    public static SecurityStamp NewStamp(Instant now) => new(Guid.NewGuid(), now);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
