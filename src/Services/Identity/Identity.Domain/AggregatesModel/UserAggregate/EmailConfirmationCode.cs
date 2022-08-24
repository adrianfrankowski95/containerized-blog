using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class EmailConfirmationCode : ValueObject<EmailConfirmationCode>
{
    private readonly static Duration _validityDuration = Duration.FromHours(1);
    private readonly Guid? _value;
    public Instant? IssuedAt { get; }
    public Instant? ValidUntil => IssuedAt?.Plus(_validityDuration);
    private readonly static EmailConfirmationCode _empty = new();
    public static EmailConfirmationCode Empty => _empty;

    private EmailConfirmationCode()
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

    private bool IsEmpty() => _value is null;
    private bool IsExpired(Instant now) => IsEmpty()
        ? throw new IdentityDomainException("Email confirmation code must not be empty.")
        : now > ValidUntil;

    public void Verify(EmailConfirmationCode providedCode, Instant now)
    {
        if (providedCode is null)
            throw new IdentityDomainException("Provided email confirmation code must not be null.");

        // Don't reveal that the email confirmation code has not been requested
        if (IsEmpty())
            throw new IdentityDomainException("The email confirmation code is invalid.");

        if (IsExpired(now))
            throw new IdentityDomainException("The email confirmation code has expired.");

        if (!Equals(providedCode))
            throw new IdentityDomainException("The email confirmation code is invalid.");
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
