using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordHash : ValueObject<PasswordHash>
{
    private readonly NonEmptyString _value;

    private PasswordHash(NonEmptyString value)
    {
        if(value is null)
            throw new ArgumentNullException("Password hash must not be null.");

        _value = value;
    }

    public static PasswordHash FromPassword(Password password, IPasswordHasher passwordHasher)
    {
        if(password is null)
            throw new ArgumentNullException("Password must not be null.");

        if(passwordHasher is null)
            throw new ArgumentNullException("Password hasher must not be null.");

        return new(passwordHasher.HashPassword(password));
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
