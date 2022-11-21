using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct SecurityStamp
{
    private readonly static Duration _validationInterval = Duration.FromMinutes(30);
    public required Guid Value { get; init; }
    public required Instant IssuedAt { get; init; }

    public SecurityStampValidationResult Validate(NonEmptyString otherStamp, Instant now)
    {
        if (IssuedAt.Plus(_validationInterval) < now)
            return SecurityStampValidationResult.NoValidationNeeded;

        return string.Equals(Value.ToString(), otherStamp, StringComparison.Ordinal)
            ? SecurityStampValidationResult.Valid
            : SecurityStampValidationResult.Invalid;
    }

    public SecurityStamp ExtendExpiration(Instant now) => new(Value, now.Plus(_validationInterval));

    [SetsRequiredMembers]
    private SecurityStamp(Guid value, Instant now)
    {
        if (value.Equals(Guid.Empty))
            throw new ArgumentException("Security stamp must not be empty.");

        Value = value;
        IssuedAt = now;
    }

    public override string ToString() => Value.ToString();

    public static SecurityStamp NewStamp(Instant now) => new(Guid.NewGuid(), now);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not SecurityStamp second)
            return false;

        return this.Value.Equals(second.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();
}
