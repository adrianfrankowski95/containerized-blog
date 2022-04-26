using BCrypt.Net;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class BcryptPasswordHasher : IPasswordHasher
{
    private readonly PasswordOptions _options;

    public BcryptPasswordHasher(IOptionsMonitor<PasswordOptions> options)
    {
        _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
    }
    public bool VerifyPassword(string password, string passwordHash)
        => BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, HashType.SHA512);

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.EnhancedHashPassword(password, _options.HashWorkFactor, HashType.SHA512);
    public bool CheckPasswordNeedsRehash(string passwordHash)
        => BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, _options.HashWorkFactor);

}
