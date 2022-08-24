using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class NonNegativeInt : ValueObject<NonNegativeInt>
{
    private readonly int _value;

    public NonNegativeInt(int value)
    {
        if (value < 0)
            throw new ArgumentException($"Value cannot be negative.");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }

    public static implicit operator NonNegativeInt(int value) => new(value);
    public static implicit operator int(NonNegativeInt value) => value._value;
    public static NonNegativeInt operator ++(NonNegativeInt a) => new(a._value + 1);
    public static bool operator >=(NonNegativeInt a, int b) => a._value >= b;
    public static bool operator <=(NonNegativeInt a, int b) => a._value <= b;
}
