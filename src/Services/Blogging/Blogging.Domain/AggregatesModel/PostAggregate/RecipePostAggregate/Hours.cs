using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Hours : ValueObject<Hours>, IComparable<Hours>
{
    private int Value { get; }

    public Hours(int hours)
    {
        if (hours < 0)
            throw new BloggingDomainException($"{nameof(Hours)} cannot be negative");

        Value = hours;
    }

    public Minutes ToMinutes() => new(Value * 60);
    public static Hours FromMinutes(Minutes minutes) => minutes.ToHours();

    public static bool operator >(Hours a, Hours b) => a.CompareTo(b) > 0;
    public static bool operator <(Hours a, Hours b) => a.CompareTo(b) < 0;
    public static bool operator >=(Hours a, Hours b) => a.CompareTo(b) >= 0;
    public static bool operator <=(Hours a, Hours b) => a.CompareTo(b) <= 0;
    public static bool operator ==(Hours a, Hours b) => a.CompareTo(b) == 0;
    public static bool operator !=(Hours a, Hours b) => a.CompareTo(b) != 0;

    public int CompareTo(Hours? other) => Value.CompareTo(other?.Value ?? 0);

    public override bool Equals(object? second)
    {
        if (second is not Hours hour)
            return false;

        return Equals(hour);
    }

    public override bool Equals(Hours? second) => Value == (second?.Value ?? 0);

    public override int GetHashCode() => Value.GetHashCode();

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}
public static class HoursExtensions
{
    public static Hours Hours(this int value) => new(value);
}

