using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Username : ValueObject<Username>
{
    private static readonly NonNegativeInt _minLength = 3;
    private static readonly NonNegativeInt _maxLength = 20;
    private static readonly NonEmptyString _allowedCharacters = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";
    private readonly NonEmptyString _value;
    private static bool IsLongEnough(NonEmptyString input, NonNegativeInt minLength) => input.Length >= minLength;
    private static bool IsShortEnough(NonEmptyString input, NonNegativeInt maxLength) => input.Length >= maxLength;
    private static bool IsAllowedChar(char c, NonEmptyString allowedCharacters) => allowedCharacters.Contains(c);

    // TODO: check if username is unique in application layer using IUserRepository
    public Username(NonEmptyString value)
    {
        if (!IsLongEnough(value, _minLength))
            throw new IdentityDomainException($"Username must be at least {_minLength} characters long.");

        if (!IsShortEnough(value, _maxLength))
            throw new IdentityDomainException($"Username must be maximum {_maxLength} characters long.");

        if (value.Any(c => !IsAllowedChar(c, _allowedCharacters)))
            throw new IdentityDomainException($"Username contains forbidden characters: {string.Join(", ", value.Where(c => !IsAllowedChar(c, _allowedCharacters)))}");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
    public static implicit operator string(Username value) => value._value;
}
