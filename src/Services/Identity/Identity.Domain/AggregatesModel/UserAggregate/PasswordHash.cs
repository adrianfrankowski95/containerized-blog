using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordHash : ValueObject<PasswordHash>
{
    private readonly NonEmptyString _value;

    public PasswordHash(Password password, IPasswordHasher passwordHasher)
    {
        _value = passwordHasher.HashPassword(password);
    }

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
