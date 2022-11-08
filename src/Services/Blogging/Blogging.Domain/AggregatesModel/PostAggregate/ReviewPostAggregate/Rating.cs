using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
public sealed class Rating : ValueObject<Rating>, IComparable<Rating>, IValidatable
{
    public int Value { get; }

    // EF Core
    public Rating()
    {
        Value = 0;
    }

    public Rating(int value)
    {
        if (value < 0)
            throw new BloggingDomainException($"{nameof(Value)} cannot be lower than 0");

        if (value > 6)
            throw new BloggingDomainException($"{nameof(Value)} cannot be higher than 6");

        Value = value;
    }
    public static bool operator >(Rating a, Rating b) => a.CompareTo(b) > 0;
    public static bool operator <(Rating a, Rating b) => a.CompareTo(b) < 0;
    public static bool operator >=(Rating a, Rating b) => a.CompareTo(b) >= 0;
    public static bool operator <=(Rating a, Rating b) => a.CompareTo(b) <= 0;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }

    public int CompareTo(Rating? other)
    {
        return Value.CompareTo(other?.Value);
    }

    public void Validate()
    {
        if (Value < 0)
            throw new BloggingDomainException($"{nameof(Value)} cannot be lower than 0");

        if (Value > 6)
            throw new BloggingDomainException($"{nameof(Value)} cannot be higher than 6");
    }
}

public static class RatingExtensions
{
    public static Rating Rating(this int value) => new(value);
}