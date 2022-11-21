using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct NonPastInstant
{
    public Instant Value { get; init; }

    [SetsRequiredMembers]
    public NonPastInstant(Instant value, Instant now)
    {
        if (value < now)
            throw new ArgumentNullException($"Value cannot be in the past.");

        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not NonPastInstant second)
            return false;

        return this.Value.Equals(second.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator Instant(NonPastInstant value) => value.Value;
}
