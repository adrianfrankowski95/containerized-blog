using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class NonPastInstant : ValueObject<NonPastInstant>
{
    private readonly Instant _value;

    public NonPastInstant(Instant value, Instant now)
    {
        if (value < now)
            throw new ArgumentNullException($"Value cannot be in the past.");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
    public static implicit operator Instant(NonPastInstant value) => value._value;
}
