using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct EmailConfirmationCode
{
    private readonly static Duration _validityDuration = Duration.FromHours(1);
    private readonly Guid? _value;
    public Instant? IssuedAt { get; }
    public Instant? ValidUntil => IssuedAt?.Plus(_validityDuration);
    private readonly static EmailConfirmationCode _empty = new();
    public static EmailConfirmationCode Empty => _empty;

    public EmailConfirmationCode()
    {
        _value = null;
        IssuedAt = null;
    }

    private EmailConfirmationCode(Guid value, Instant now)
    {
        if (value.Equals(Guid.Empty))
            throw new ArgumentException("Email confirmation code must not be empty.");

        _value = value;
        IssuedAt = now;
    }

    public static EmailConfirmationCode NewCode(Instant now) => new(Guid.NewGuid(), now);

    public bool IsEmpty => _value is null;
    private bool IsExpired(Instant now) => IsEmpty
        ? throw new IdentityDomainException("Email confirmation code must not be empty.")
        : now > ValidUntil;

    public void Verify(NonEmptyString providedCode, Instant now)
    {
        // Don't reveal that the email confirmation code has not been requested
        if (IsEmpty)
            throw new IdentityDomainException("The email confirmation code is invalid.");

        if (IsExpired(now))
            throw new EmailConfirmationCodeExpirationException("The email confirmation code has expired.");

        if (!string.Equals(ToString(), providedCode, StringComparison.Ordinal))
            throw new IdentityDomainException("The email confirmation code is invalid.");
    }

    public override string ToString() => _value?.ToString() ?? "";

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not EmailConfirmationCode second)
            return false;

        return this._value?.Equals(second._value) ?? second._value is null;
    }

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}
