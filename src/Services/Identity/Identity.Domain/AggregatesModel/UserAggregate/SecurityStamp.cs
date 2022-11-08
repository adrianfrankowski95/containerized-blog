using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class SecurityStamp : ValueObject<SecurityStamp>
{
    private readonly static Duration _validationInterval = Duration.FromMinutes(30);
    private readonly Guid _value;
    public Instant IssuedAt { get; }

    public SecurityStampValidationResult Validate(NonEmptyString otherStamp, Instant now)
    {
        if (IssuedAt.Plus(_validationInterval) < now)
            return SecurityStampValidationResult.NoValidationNeeded;

        return string.Equals(_value.ToString(), otherStamp, StringComparison.Ordinal)
            ? SecurityStampValidationResult.Valid
            : SecurityStampValidationResult.Invalid;
    }

    public SecurityStamp ExtendExpiration(Instant now) => new(_value, now.Plus(_validationInterval));

    private SecurityStamp(Guid value, Instant now)
    {
        if (value.Equals(Guid.Empty))
            throw new ArgumentException("Security stamp must not be empty.");

        _value = value;
        IssuedAt = now;
    }

    public override string ToString() => _value.ToString();

    public static SecurityStamp NewStamp(Instant now) => new(Guid.NewGuid(), now);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
