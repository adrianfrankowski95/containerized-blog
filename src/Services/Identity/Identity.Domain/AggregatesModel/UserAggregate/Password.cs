using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Password : ValueObject<Password>
{
    private static readonly NonNegativeInt _minLength = 8;
    private readonly NonEmptyString _value;
    private static bool IsLongEnough(NonEmptyString input, NonNegativeInt minLength) => input.Length >= minLength;
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';
    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';
    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);

    public Password(NonEmptyString value)
    {
        if (value is null)
            throw new IdentityDomainException("Password must not be null.");

        if (!IsLongEnough(value, _minLength))
            throw new IdentityDomainException($"Password must be at least {_minLength} characters long.");

        if (value.Any(c => !IsDigit(c)))
            throw new IdentityDomainException("Password must contain at least one digit.");

        if (value.Any(c => !IsUppercase(c)))
            throw new IdentityDomainException("Password must contain at least one uppercase character.");

        if (value.Any(c => !IsLowercase(c)))
            throw new IdentityDomainException("Password must contain at least one lowercase character.");

        if (value.Any(c => !IsNonAlphanumeric(c)))
            throw new IdentityDomainException("Password must contain at least one non-alphanumeric character.");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
