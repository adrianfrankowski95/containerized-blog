using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class FailedLoginAttemptsCount : ValueObject<FailedLoginAttemptsCount>
{
    private static readonly Duration _validityDuration = Duration.FromMinutes(5);
    private static readonly NonNegativeInt _maxAllowed = 5;
    private readonly NonNegativeInt _count;
    public Instant? LastFailAt { get; }
    public Instant? ValidUntil => LastFailAt?.Plus(_validityDuration);
    private static readonly FailedLoginAttemptsCount _none = new();
    public static FailedLoginAttemptsCount None => _none;

    private FailedLoginAttemptsCount()
    {
        _count = 0;
    }

    private FailedLoginAttemptsCount(NonNegativeInt count, Instant now)
    {
        if (count is null)
            throw new IdentityDomainException("Login attempts count must not be null.");

        _count = count;
        LastFailAt = now;
    }

    public bool IsMaxAllowed() => _count == _maxAllowed;
    public FailedLoginAttemptsCount Increment(Instant now)
    {
        if (IsMaxAllowed())
            throw new IdentityDomainException($"Exceeded maximum allowed failed login attempts.");

        return new(_count + 1, now);
    }
    public bool IsEmpty => _count == 0;
    public bool IsExpired(Instant now) => IsEmpty
        ? throw new IdentityDomainException("Failed login attempts count must not be empty.")
        : now > ValidUntil;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _count;
    }

    public override bool Equals(FailedLoginAttemptsCount? second) => base.Equals(second);
    public override bool Equals(object? second) => base.Equals(second);
    public override int GetHashCode() => base.GetHashCode();
}