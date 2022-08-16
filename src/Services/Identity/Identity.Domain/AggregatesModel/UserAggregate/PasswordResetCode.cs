using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordResetCode : ValueObject<PasswordResetCode>
{
    // Without I,l for better legibility
    private readonly static NonEmptyString _allowedCharacters = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    private readonly static Duration _validityDuration = Duration.FromHours(1);
    private readonly static NonNegativeInt _length = 6;
    private readonly static PasswordResetCode _empty = new();
    private readonly string? _value;
    public Instant? IssuedAt { get; }

    private PasswordResetCode()
    { }

    private PasswordResetCode(NonEmptyString value)
    {
        if (value.Length != _length || value.Any(c => !_allowedCharacters.Contains(c)))
            throw new IdentityDomainException("Invalid password reset code format.");

        _value = value;
        IssuedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public static PasswordResetCode NewCode()
    {
        var rnd = Random.Shared;
        var code = new char[_length];

        for (int i = 0; i < _length; ++i)
        {
            code[i] = _allowedCharacters[rnd.Next(0, _allowedCharacters.Length)];
        }

        return new(new string(code));
    }

    public override string? ToString() => _value;

    public static PasswordResetCode EmptyCode() => _empty;

    //public static PasswordResetCode FromExiting(NonEmptyString code, Instant? issuedAt = null) => new(code, issuedAt);

    public bool IsEmpty() => string.IsNullOrWhiteSpace(_value);

    private bool IsExpired() => IsEmpty()
        ? throw new IdentityDomainException("Error checking password reset code expiration.")
        : SystemClock.Instance.GetCurrentInstant() > IssuedAt?.Plus(_validityDuration);

    public void Verify(PasswordResetCode providedCode)
    { 
        // Don't reveal that the password reset code has not been requested
        if(IsEmpty())
            throw new IdentityDomainException("The password reset code is invalid.");

        if(IsExpired())
            throw new IdentityDomainException("The password reset code has expired.");

        if(!Equals(providedCode))
            throw new IdentityDomainException("The password reset code is invalid.");
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
