using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct Username
{
    public const int MinLength = 3;
    public const int MaxLength = 20;
    public const string AllowedCharacters = "aąbcćdeęfghijklłmnńopqrsśtuóvwxyzźż.-_1234567890";
    public required NonEmptyString Value { get; init; }
    private static bool IsLongEnough(NonEmptyString input) => input.Length >= MinLength;
    private static bool IsShortEnough(NonEmptyString input) => input.Length >= MaxLength;
    private static bool IsAllowedChar(char c) => AllowedCharacters.Contains(c);

    [SetsRequiredMembers]
    public Username(NonEmptyString value)
    {
        if (!IsLongEnough(value))
            throw new IdentityDomainException($"Username must be at least {MinLength} characters long.");

        if (!IsShortEnough(value))
            throw new IdentityDomainException($"Username must be maximum {MaxLength} characters long.");

        if (value.Any(c => !IsAllowedChar(c)))
            throw new IdentityDomainException($"Username contains forbidden characters: {string.Join(", ", value.Where(c => !IsAllowedChar(c)))}");

        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not Username second)
            return false;

        return this.Value.Equals(second.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(Username value) => value.Value;
}
