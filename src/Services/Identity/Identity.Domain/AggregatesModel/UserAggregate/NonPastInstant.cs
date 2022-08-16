using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Identity.Domain.AggregatesModel.UserAggregate;

public class NonPastInstant : ValueObject<NonPastInstant>
{
    private readonly Instant _value;

    public NonPastInstant(Instant value)
    {
        if (value < SystemClock.Instance.GetCurrentInstant())
            throw new ArgumentNullException($"Value cannot be in the past.");

        _value = value;
    }

    public static implicit operator NonPastInstant(Instant value) => new(value);
    public static implicit operator Instant(NonPastInstant value) => value._value;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
