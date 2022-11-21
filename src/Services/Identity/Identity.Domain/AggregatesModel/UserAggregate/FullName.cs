using System.Diagnostics.CodeAnalysis;

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