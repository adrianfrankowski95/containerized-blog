using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct FailedLoginAttempts
{
    private static readonly Duration _validityDuration = Duration.FromMinutes(5);
    private static readonly NonNegativeInt _maxAllowed = 5;
    public required NonNegativeInt Count { get; init; }
    public Instant? LastFailAt { get; }
    public Instant? ValidUntil => LastFailAt?.Plus(_validityDuration);
    private static readonly FailedLoginAttempts _none = new();
    public static FailedLoginAttempts None => _none;

    [SetsRequiredMembers]
    public FailedLoginAttempts()
    {
        Count = 0;
    }

    [SetsRequiredMembers]
    private FailedLoginAttempts(NonNegativeInt count, Instant now)
    {
        Count = count;
        LastFailAt = now;
    }

    public bool IsMaxAllowed() => Count == _maxAllowed;
    public FailedLoginAttempts Increment(Instant now)
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

        if (obj is not FailedLoginAttempts second)
            return false;

        return this.Count == second.Count;
    }

    public override int GetHashCode() => Count.GetHashCode();
}