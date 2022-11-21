using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct EmailAddress
{
    private readonly NonEmptyString _value;
    public bool IsConfirmed { get; init; }

    public EmailAddress(NonEmptyString value)
    {
        if (!new EmailAddressAttribute().IsValid(value))
            throw new IdentityDomainException("Invalid email address format.");

        _value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not EmailAddress second)
            return false;

        return this._value.Equals(second._value) && this.IsConfirmed.Equals(second.IsConfirmed);
    }

    public override int GetHashCode() => _value.GetHashCode();

    public EmailAddress Confirm() => new(_value) { IsConfirmed = true };
    public override string ToString() => _value;
    public static implicit operator EmailAddress(NonEmptyString value) => new(value);
    public static implicit operator string(EmailAddress value)
        => value._value;
}
