using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class FailedLoginAttemptsCount : ValueObject<FailedLoginAttemptsCount>
{
    private static readonly FailedLoginAttemptsCount _none = new();
    private readonly NonNegativeInt _count;
    public static FailedLoginAttemptsCount None => _none;
    public Instant? LastNotedAt { get; }
    public Instant? ValidUntil => LastNotedAt?.Plus(Duration.FromMinutes(5));
    private FailedLoginAttemptsCount()
    {
        _count = 0;
    }

    private FailedLoginAttemptsCount(NonNegativeInt count)
    {
        if(count is null)
            throw new ArgumentNullException("Login attempts count must not be null.");

        _count = count;
        LastNotedAt = SystemClock.Instance.GetCurrentInstant();
    }
    
    public bool IsMaxAllowed() => _count == 5;

    public FailedLoginAttemptsCount Increment()
    {
        if (IsMaxAllowed())
            throw new IdentityDomainException($"Exceeded maximum allowed failed login attempts.");

        return new(_count + 1);
    }

    public bool IsEmpty() => _count == 0;
    public bool IsExpired() => IsEmpty()
        ? throw new IdentityDomainException("Failed login attempts count must not be empty.")
        : SystemClock.Instance.GetCurrentInstant() > ValidUntil;

    public override bool Equals(FailedLoginAttemptsCount? second) => base.Equals(second);
    public override bool Equals(object? second) => base.Equals(second);
    public override int GetHashCode() => base.GetHashCode();

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _count;
    }
}