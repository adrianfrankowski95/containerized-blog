#pragma warning disable CS8618

using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public abstract class PasswordHasher
{

    // NewHash is assigned in static constructor at the very beginning, therefore it will never be null
    protected static Func<NonEmptyString, PasswordHash> NewHash { get; private set; }
    public abstract PasswordHash HashPassword(Password password);
    public abstract bool VerifyPasswordHash(NonEmptyString password, PasswordHash? passwordHash);

    public class PasswordHash : ValueObject<PasswordHash>
    {
        private readonly NonEmptyString _value;

        static PasswordHash()
        {
            NewHash = value => new PasswordHash(value);
        }

        private PasswordHash(NonEmptyString value)
        {
            if (value is null)
                throw new IdentityDomainException("Password hash must not be null.");

            _value = value;
        }

        public static implicit operator string(PasswordHash value) => value?._value ?? throw new IdentityDomainException("Password hash must not be null.");
        protected override IEnumerable<object?> GetEqualityCheckAttributes()
        {
            yield return _value;
        }
    }
}
