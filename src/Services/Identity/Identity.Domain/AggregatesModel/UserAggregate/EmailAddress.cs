using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class EmailAddress : ValueObject<EmailAddress>
{
    private readonly NonEmptyString _value;
    public bool IsConfirmed { get; private set; }

    public EmailAddress(NonEmptyString value)
    {
        if (!new EmailAddressAttribute().IsValid(value))
            throw new IdentityDomainException("Invalid email address format.");

        _value = value;
    }

    public EmailAddress Confirm() => new(_value) { IsConfirmed = true };

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }

    public override string ToString() => _value;

    public static implicit operator EmailAddress(string value) => new(value);
    public static implicit operator string(EmailAddress value) => value._value;
}
