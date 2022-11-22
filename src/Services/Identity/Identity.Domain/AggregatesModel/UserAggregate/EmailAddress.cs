using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct EmailAddress
{
    public required NonEmptyString Value { get; init; }
    public bool IsConfirmed { get; init; }

    [SetsRequiredMembers]
    public EmailAddress(NonEmptyString value)
    {
        if (!new EmailAddressAttribute().IsValid(value))
            throw new IdentityDomainException("Invalid email address format.");

        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not EmailAddress second)
            return false;

        return this.Value.Equals(second.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public EmailAddress Confirm() => new(Value) { IsConfirmed = true };
    public override string ToString() => Value;
    public static implicit operator EmailAddress(NonEmptyString value) => new(value);
    public static implicit operator string(EmailAddress value)
        => value.Value;
}
