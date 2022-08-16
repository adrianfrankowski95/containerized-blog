using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordResetCode : ValueObject<PasswordResetCode>
{
    // Without I,l for better legibility
    private const string AllowedCharacters = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    private const int Length = 6;
    private readonly static PasswordResetCode _empty = new();
    private readonly string? _value;
    public Instant? IssuedAt { get; }
    private Instant? ValidUntil => IssuedAt?.Plus(Duration.FromHours(1));

    public PasswordResetCode()
    {
        _value = null;
        IssuedAt = null;
    }

    private PasswordResetCode(NonEmptyString value)
    {
        if (value.Length != Length || value.Any(c => !AllowedCharacters.Contains(c)))
            throw new IdentityDomainException("Invalid password reset code format.");

        _value = value;
        IssuedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public static PasswordResetCode NewCode()
    {
        var rnd = Random.Shared;
        var code = new char[Length];

        for (int i = 0; i < Length; ++i)
        {
            code[i] = AllowedCharacters[rnd.Next(0, AllowedCharacters.Length)];
        }

        return new(new string(code));
    }

    public static PasswordResetCode EmptyCode() => _empty;

    public bool IsEmpty() => string.IsNullOrWhiteSpace(_value);
    private bool IsExpired() => IsEmpty()
        ? throw new IdentityDomainException("Password must not be empty.")
        : SystemClock.Instance.GetCurrentInstant() > ValidUntil;

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
