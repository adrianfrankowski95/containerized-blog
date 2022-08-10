using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Password : ValueObject<Password>
{
    private readonly static NonNegativeInt _length = 8;
    private readonly NonEmptyString _value;

    public Password(NonEmptyString value)
    {
        if (!value.Any(c => IsDigit(c)))
            throw new IdentityDomainException("Password must contain at least one digit.");

        if (!value.Any(c => IsUppercase(c)))
            throw new IdentityDomainException("Password must contain at least one uppercase character.");

        if (!value.Any(c => IsLowercase(c)))
            throw new IdentityDomainException("Password must contain at least one lowercase character.");

        if (!value.Any(c => IsNonAlphanumeric(c)))
            throw new IdentityDomainException("Password must contain at least one non alphanumeric character.");

        if (value.Length < _length)
            throw new IdentityDomainException($"Password must be at least {_length} characters long.");

        _value = value;
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';

    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';

    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);

    public static implicit operator Password(NonEmptyString value) => new(value);
    public static implicit operator NonEmptyString(Password value) => value._value;

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
