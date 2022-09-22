using BCrypt.Net;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class BcryptPasswordHasher : PasswordHasher
{
    private const HashType HashType = BCrypt.Net.HashType.SHA512;
    public override PasswordHash HashPassword(Password password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        return _newHash(BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType));
    }

    public override bool VerifyPasswordHash(NonEmptyString password, PasswordHash? passwordHash)
        => password is not null && passwordHash is not null && BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, HashType);
}