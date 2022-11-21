using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct FailedLoginAttemptsCount
{
    private static readonly Duration _validityDuration = Duration.FromMinutes(5);
    private static readonly NonNegativeInt _maxAllowed = 5;
    public required NonNegativeInt Count { get; init; }
    public Instant? LastFailAt { get; }
    public Instant? ValidUntil => LastFailAt?.Plus(_validityDuration);
    private static readonly FailedLoginAttemptsCount _none = new();
    public static FailedLoginAttemptsCount None => _none;

    [SetsRequiredMembers]
    public FailedLoginAttemptsCount()
    {
        Count = 0;
    }

    [SetsRequiredMembers]
    private FailedLoginAttemptsCount(NonNegativeInt count, Instant now)
    {
        Count = count;
        LastFailAt = now;
    }

    public bool IsMaxAllowed() => Count == _maxAllowed;
    public FailedLoginAttemptsCount Increment(Instant now)
    {
        if (IsMaxAllowed())
            throw new IdentityDomainException($"Exceeded maximum allowed failed login attempts.");

        return new(Count + 1, now);
    }
    public bool IsEmpty => Count == 0;
    public bool IsExpired(Instant now) => IsEmpty
        ? throw new IdentityDomainException("Failed login attempts count must not be empty.")
        : now > ValidUntil;

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not FailedLoginAttemptsCount second)
            return false;

        return this.Count == second.Count;
    }

    public override int GetHashCode() => Count.GetHashCode();
}