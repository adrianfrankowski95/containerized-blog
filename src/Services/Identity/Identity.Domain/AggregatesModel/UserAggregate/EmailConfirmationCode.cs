using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class EmailConfirmationCode : ValueObject<EmailConfirmationCode>
{
    private readonly static Duration _validityDuration = Duration.FromHours(1);
    private readonly static EmailConfirmationCode _empty = new();
    private readonly Guid? _value;
    public Instant? IssuedAt { get; }

    private EmailConfirmationCode()
    { }

    private EmailConfirmationCode(Guid value)
    {
        _value = value;
        IssuedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public Guid? ToGuid() => _value;

    public static EmailConfirmationCode NewCode() => new(Guid.NewGuid());

    public static EmailConfirmationCode EmptyCode() => _empty;

    //public static EmailConfirmationCode FromExiting(Guid code, Instant? issuedAt = null) => new(code, issuedAt);

    private bool IsEmpty() => _value is null;

    private bool IsExpired() => IsEmpty()
        ? throw new IdentityDomainException("Error checking email confirmation code expiration.")
        : SystemClock.Instance.GetCurrentInstant() > IssuedAt?.Plus(_validityDuration);

    public bool Verify(EmailConfirmationCode providedCode) => !IsEmpty() && !providedCode.IsEmpty() && !IsExpired() && Equals(providedCode);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
