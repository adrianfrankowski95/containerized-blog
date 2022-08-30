using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public abstract class PasswordHasher
{
    protected static Func<NonEmptyString, PasswordHash> _newHash;
    public abstract PasswordHash HashPassword(Password password);

    public class PasswordHash : ValueObject<PasswordHash>
    {
        private readonly NonEmptyString _value;

        static PasswordHash()
        {
            _newHash = value => new PasswordHash(value);
        }

        private PasswordHash(NonEmptyString value)
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
}
