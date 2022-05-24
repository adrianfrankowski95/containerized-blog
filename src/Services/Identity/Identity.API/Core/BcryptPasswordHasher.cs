using BCrypt.Net;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class BcryptPasswordHasher : IPasswordHasher
{
    private readonly IOptionsMonitor<PasswordOptions> _options;

    public BcryptPasswordHasher(IOptionsMonitor<PasswordOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    public PasswordVerificationResult VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash));

        if (BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, HashType.SHA512))
        {
            if (CheckPasswordNeedsRehash(passwordHash))
                return PasswordVerificationResult.SuccessNeedsRehash;

            return PasswordVerificationResult.Success;
        }

        return PasswordVerificationResult.Fail;
    }

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.EnhancedHashPassword(password, _options.CurrentValue.HashWorkFactor, HashType.SHA512);

    private bool CheckPasswordNeedsRehash(string passwordHash)
        => BCrypt.Net.BCrypt.PasswordNeedsRehash(passwordHash, _options.CurrentValue.HashWorkFactor);

}
