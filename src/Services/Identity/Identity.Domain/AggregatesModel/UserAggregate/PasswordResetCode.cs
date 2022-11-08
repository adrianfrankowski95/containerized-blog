using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordResetCode : ValueObject<PasswordResetCode>
{
    // Without I,l for better legibility
    private const string AllowedCharacters = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    private const int Length = 6;
    private readonly static Duration _validityDuration = Duration.FromHours(1);
    private readonly string? _value;
    public Instant? IssuedAt { get; }
    public Instant? ValidUntil => IssuedAt?.Plus(_validityDuration);
    private readonly static PasswordResetCode _empty = new();
    public static PasswordResetCode Empty => _empty;

    private PasswordResetCode()
    {
        _value = null;
        IssuedAt = null;
    }

    private PasswordResetCode(NonEmptyString value, Instant now)
    {
        if (value is null)
            throw new ArgumentNullException("Password reset code must not be null.");

        if (value.Length != Length || value.Any(c => !AllowedCharacters.Contains(c)))
            throw new IdentityDomainException("Invalid password reset code format.");

        _value = value;
        IssuedAt = now;
    }

    public static PasswordResetCode NewCode(Instant now)
    {
        var rnd = Random.Shared;
        var code = new char[Length];

        for (int i = 0; i < Length; ++i)
        {
            code[i] = AllowedCharacters[rnd.Next(0, AllowedCharacters.Length)];
        }

        return new(new string(code), now);
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(_value);
    private bool IsExpired(Instant now) => IsEmpty
        ? throw new IdentityDomainException("Password must not be empty.")
        : now > ValidUntil;

    public void Verify(NonEmptyString providedCode, Instant now)
    {
        if (providedCode is null)
            throw new ArgumentNullException("Provided password reset code must not be null.");

        // Don't reveal that the password reset code has not been requested
        if (IsEmpty)
            throw new IdentityDomainException("The password reset code is invalid.");

        if (IsExpired(now))
            throw new PasswordResetCodeExpirationException("The password reset code has expired.");

        if (!string.Equals(ToString(), providedCode, StringComparison.Ordinal))
            throw new IdentityDomainException("The password reset code is invalid.");
    }
    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }

    public static implicit operator string(PasswordResetCode value) => value?._value ?? throw new IdentityDomainException("Password reset code must not be null");
}
