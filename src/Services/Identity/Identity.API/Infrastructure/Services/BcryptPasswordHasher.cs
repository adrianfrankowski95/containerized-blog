using BCrypt.Net;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class BcryptPasswordHasher : PasswordHasher
{
    public override PasswordHash HashPassword(Password password)
        => _newHash(BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType.SHA512));
}