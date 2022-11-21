#pragma warning disable CS8618

using System.Diagnostics.CodeAnalysis;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public abstract class PasswordHasher
{

    // NewHash is assigned in static constructor at the very beginning, therefore it will never be null
    protected static Func<NonEmptyString, PasswordHash> NewHash { get; private set; }
    public abstract PasswordHash HashPassword(Password password);
    public abstract bool VerifyPasswordHash(NonEmptyString password, PasswordHash? passwordHash);

    public readonly struct PasswordHash
    {
        private readonly NonEmptyString _value;

        static PasswordHash()
        {
            NewHash = value => new PasswordHash(value);
        }

        [SetsRequiredMembers]
        private PasswordHash(NonEmptyString value)
        {
            _value = value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is null)
                return false;

            if (obj is not PasswordHash second)
                return false;

            return this._value.Equals(second._value);
        }

        public override int GetHashCode() => _value.GetHashCode();

        public static implicit operator string(PasswordHash value) => value._value;
        public static implicit operator PasswordHash(NonEmptyString value) => new(value);
    }
}
