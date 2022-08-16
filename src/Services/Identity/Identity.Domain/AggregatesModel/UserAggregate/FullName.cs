using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class FullName : ValueObject<FullName>
{
    public NonEmptyString FirstName { get; }
    public NonEmptyString LastName { get; }
    public FullName(NonEmptyString firstName, NonEmptyString lastName)
    {
        if(firstName is null)
            throw new ArgumentNullException("First name must not be null.");

        if(lastName is null)
            throw new ArgumentNullException("Last name must not be null.");

        FirstName = firstName;
        LastName = lastName;
    }
    public override string ToString() => FirstName + " " + LastName;
    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return FirstName;
        yield return LastName;
    }
}