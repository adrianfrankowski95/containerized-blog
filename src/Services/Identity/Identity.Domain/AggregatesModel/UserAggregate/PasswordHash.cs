using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class PasswordHash : ValueObject<PasswordHash>
{
    private readonly NonEmptyString _value;

    private PasswordHash(NonEmptyString value)
    {
        _value = value;
    }

    public static PasswordHash FromPassword(Password password, IPasswordHasher passwordHasher)
        => new(passwordHasher.HashPassword(password));

    public override string ToString() => _value;

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return _value;
    }
}
