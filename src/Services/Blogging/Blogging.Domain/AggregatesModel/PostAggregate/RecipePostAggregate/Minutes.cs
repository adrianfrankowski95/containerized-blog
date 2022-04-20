using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Minutes : ValueObject<Minutes>, IComparable<Minutes>
{
    private int Value { get; }

    public Minutes(int minutes)
    {
        if (minutes == null)
            throw new BloggingDomainException($"{nameof(Minutes)} cannot be null");

        if (minutes < 0)
            throw new BloggingDomainException($"{nameof(Minutes)} cannot be negative");

        Value = minutes;
    }

    public Hours ToHours() => new(Value / 60);
    public int AsInt() => Value;
    public static Minutes FromHours(Hours hours) => hours.ToMinutes();
    public Minutes RemainderFromHours() => new(Value % 60);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Value;
    }

    public static bool operator >(Minutes a, Minutes b) => a.CompareTo(b) > 0;
    public static bool operator <(Minutes a, Minutes b) => a.CompareTo(b) < 0;
    public static bool operator >=(Minutes a, Minutes b) => a.CompareTo(b) >= 0;
    public static bool operator <=(Minutes a, Minutes b) => a.CompareTo(b) <= 0;
    public static Minutes operator +(Minutes a, Minutes b) => (a.Value + b.Value).Minutes();

    public int CompareTo(Minutes other)
    {
        return Value.CompareTo(other.Value);
    }
}
public static class MinutesExtensions
{
    public static Minutes Minutes(this int value) => new(value);
}

