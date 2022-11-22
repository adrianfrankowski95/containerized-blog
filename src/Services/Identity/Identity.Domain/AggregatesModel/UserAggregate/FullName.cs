using System.Diagnostics.CodeAnalysis;
using Blog.Services.Identity.Domain.Exceptions;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public readonly struct FullName
{
    public required NonEmptyString FirstName { get; init; }
    public required NonEmptyString LastName { get; init; }

    [SetsRequiredMembers]
    public FullName(NonEmptyString firstName, NonEmptyString lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public override string ToString() => FirstName + " " + LastName;
    public static FullName FromString(NonEmptyString fullName)
    {
        var nameSplit = fullName.Split(' ');

        if (nameSplit.Length != 2)
            throw new IdentityDomainException("Invalid full name format.");

        return new FullName(nameSplit[0], nameSplit[1]);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;

        if (obj is not FullName second)
            return false;

        return this.FirstName.Equals(second.FirstName) && this.LastName.Equals(second.LastName);
    }

    public override int GetHashCode() => HashCode.Combine(FirstName, LastName);
}