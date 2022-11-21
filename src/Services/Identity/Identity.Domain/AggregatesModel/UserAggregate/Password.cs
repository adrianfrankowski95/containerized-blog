using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct Password
{
    public const int MinLength = 8;
    private readonly NonEmptyString _value;
    private static bool IsLongEnough(NonEmptyString input) => input.Length >= MinLength;
    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';
    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';
    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);

    [SetsRequiredMembers]
    public Password(NonEmptyString password)
    {
        if (!IsLongEnough(password))
            throw new IdentityDomainException($"Password must be at least {MinLength} characters long.");

        if (password.Any(c => !IsDigit(c)))
            throw new IdentityDomainException("Password must contain at least one digit.");

        if (password.Any(c => !IsUppercase(c)))
            throw new IdentityDomainException("Password must contain at least one uppercase character.");

        if (password.Any(c => !IsLowercase(c)))
            throw new IdentityDomainException("Password must contain at least one lowercase character.");

        if (password.Any(c => !IsNonAlphanumeric(c)))
            throw new IdentityDomainException("Password must contain at least one non-alphanumeric character.");

        _value = password;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not Password second)
            return false;

        return this._value.Equals(second._value);
    }

    public override int GetHashCode() => _value.GetHashCode();

    public static implicit operator Password(NonEmptyString value) => new(value);
    public static implicit operator NonEmptyString(Password value) => value._value;
    public static implicit operator string(Password value) => value._value;
}
