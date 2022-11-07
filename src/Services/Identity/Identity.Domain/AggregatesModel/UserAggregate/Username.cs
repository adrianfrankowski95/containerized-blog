using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Username : ValueObject<Username>
{
    public const int MinLength = 3;
    public const int MaxLength = 20;
    public const string AllowedCharacters = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";
    private readonly NonEmptyString _value;
    private static bool IsLongEnough(NonEmptyString input) => input.Length >= MinLength;
    private static bool IsShortEnough(NonEmptyString input) => input.Length >= MaxLength;
    private static bool IsAllowedChar(char c) => AllowedCharacters.Contains(c);

    // TODO: check if username is unique in application layer using IUserRepository
    public Username(NonEmptyString value)
    {
        if (!IsLongEnough(value))
            throw new IdentityDomainException($"Username must be at least {MinLength} characters long.");

        if (!IsShortEnough(value))
            throw new IdentityDomainException($"Username must be maximum {MaxLength} characters long.");

        if (value.Any(c => !IsAllowedChar(c)))
            throw new IdentityDomainException($"Username contains forbidden characters: {string.Join(", ", value.Where(c => !IsAllowedChar(c)))}");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
    public static implicit operator string(Username value) => value?._value ?? throw new IdentityDomainException("Username must not be null.");
}
