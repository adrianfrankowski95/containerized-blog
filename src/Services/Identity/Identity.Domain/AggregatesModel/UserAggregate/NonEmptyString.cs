using System.Diagnostics.CodeAnalysis;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct NonEmptyString
{
    public required string Value { get; init; }
    public NonNegativeInt Length => Value.Length;
    public char this[NonNegativeInt index]
    {
        get => Value[index];
    }

    [SetsRequiredMembers]
    public NonEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not NonEmptyString second)
            return false;

        return string.Equals(this.Value, second.Value, StringComparison.Ordinal);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
    public string[] Split(char separator, StringSplitOptions options = StringSplitOptions.None)
        => Value.Split(separator, options);
    public bool Contains(char value) => Value.Contains(value);
    public bool Any(Func<char, bool> predicate) => Value.Any(predicate);
    public IEnumerable<char> Where(Func<char, bool> selector) => Value.Where(selector);
    public static implicit operator NonEmptyString(string value) => new NonEmptyString(value);
    public static implicit operator string(NonEmptyString value) => value.Value;
}
