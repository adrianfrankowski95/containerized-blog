using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class NonEmptyString : ValueObject<NonEmptyString>
{
    private readonly string _value;
    public NonNegativeInt Length => _value.Length;
    public char this[NonNegativeInt index]
    {
        get => _value[index];
    }

    public NonEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }

    public override string ToString() => _value;
    public bool Contains(char value) => _value.Contains(value);
    public bool Any(Func<char, bool> predicate) => _value.Any(predicate);

    public static implicit operator NonEmptyString(string value) => new(value);
    public static implicit operator string(NonEmptyString value) => value._value;
}
