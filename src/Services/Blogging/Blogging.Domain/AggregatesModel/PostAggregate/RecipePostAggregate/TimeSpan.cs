using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class TimeSpan : ValueObject<TimeSpan>, IComparable<TimeSpan>
{
    public Hours Hours { get; }
    public Minutes Minutes { get; }

    public TimeSpan(Hours hours, Minutes minutes)
    {
        if (hours is null)
            throw new BloggingDomainException($"{nameof(Hours)} cannot be null");

        if (minutes is null)
            throw new BloggingDomainException($"{nameof(Minutes)} cannot be null");

        if (hours < 0.Hours())
            throw new BloggingDomainException($"{nameof(Hours)} cannot be negative");

        if (minutes < 0.Minutes() || minutes > 60.Minutes())
            throw new BloggingDomainException($"{nameof(Minutes)} cannot be negative or higher than 60");

        Hours = hours;
        Minutes = minutes;
    }

    public static TimeSpan FromMinutes(Minutes minutes)
    {
        if (minutes < 0.Minutes())
            throw new BloggingDomainException($"{nameof(Minutes)} cannot be negative");

        Hours hours = minutes.ToHours();
        Minutes remainingMinutes = minutes.RemainderFromHours();

        return new TimeSpan(hours, remainingMinutes);
    }

    public Minutes ToMinutes() => Hours.ToMinutes() + Minutes;

    public static TimeSpan operator +(TimeSpan a, TimeSpan b) => FromMinutes(a.Hours.ToMinutes() + a.Minutes + b.Hours.ToMinutes() + b.Minutes);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Hours;
        yield return Minutes;
    }

    public int CompareTo(TimeSpan other)
    {
        int minutesDifference = Minutes.CompareTo(other.Minutes);
        int hoursDifference = Hours.ToMinutes().CompareTo(other.Hours.ToMinutes());

        return minutesDifference + hoursDifference;
    }
}
