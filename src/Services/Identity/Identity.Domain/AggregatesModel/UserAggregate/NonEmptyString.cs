using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

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

    public override string ToString() => _value;
    public bool Contains(char value) => _value.Contains(value);
    public bool Any(Func<char, bool> predicate) => _value.Any(predicate);
    public IEnumerable<char> Where(Func<char, bool> selector) => _value.Where(selector);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }

    public static implicit operator NonEmptyString(string value) => new(value);
    public static implicit operator string(NonEmptyString value) =>
        string.IsNullOrWhiteSpace(value?._value)
            ? throw new ArgumentNullException(nameof(value))
            : value._value;
}
