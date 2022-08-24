using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordHash : ValueObject<PasswordHash>
{
    private readonly NonEmptyString _value;

    // TODO: hash value in application layer using IPasswordHasher
    public PasswordHash(NonEmptyString value)
    {
        if (value is null)
            throw new ArgumentNullException("Password hash must not be null.");

        _value = value;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
