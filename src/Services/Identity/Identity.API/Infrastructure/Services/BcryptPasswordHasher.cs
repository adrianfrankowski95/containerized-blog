using BCrypt.Net;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class BcryptPasswordHasher : PasswordHasher
{
    private const HashType HashType = BCrypt.Net.HashType.SHA512;
    public override PasswordHash HashPassword(Password password)
    {
        return NewHash(BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType));
    }

    public override bool VerifyPasswordHash(NonEmptyString password, PasswordHash? passwordHash)
        => passwordHash is not null && BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, HashType);
}