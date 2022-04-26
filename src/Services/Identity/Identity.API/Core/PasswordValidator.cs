using Blog.Services.Identity.API.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class PasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;
    private readonly IOptionsMonitor<PasswordOptions> _options;
    private readonly IPasswordHasher _passwordHasher;

    public PasswordValidator(IUserRepository<TUser> userRepository, IOptionsMonitor<PasswordOptions> options, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }
    public async ValueTask<IdentityResult> ValidateAsync(TUser user, string password)
    {
        var passwordHash = user.PasswordHash;

        if (string.IsNullOrWhiteSpace(password) || passwordHash is null || string.IsNullOrWhiteSpace(passwordHash))
            return IdentityResult.Fail(IdentityError.MissingPassword);

        var opts = _options.CurrentValue;

        //validate password format
        if (password.Length < opts.MinLength ||
            (opts.RequireLowerCase && !password.Any(IsLowercase)) ||
            (opts.RequireUpperCase && !password.Any(IsUppercase)) ||
            (opts.RequireDigit && !password.Any(IsDigit)) ||
            (opts.RequireNonAlphanumeric && !password.Any(IsNonAlphanumeric)))
            return IdentityResult.Fail(IdentityError.InvalidPasswordFormat);

        var owner = await _userRepository.FindByIdAsync(user.Id).ConfigureAwait(false);

        if (owner is not null)
            if (!owner.PasswordHash.Equals(user.PasswordHash))
                return IdentityResult.Fail(IdentityError.InvalidPassword);

        //if checked that provided password hash belongs to its owner, verify it
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return IdentityResult.Fail(IdentityError.InvalidPassword);

        if (_passwordHasher.CheckPasswordNeedsRehash(user.PasswordHash))
            return IdentityResult.Fail(IdentityError.PasswordOkNeedsRehash);

        return IdentityResult.Success;
    }

    public static bool IsDigit(char c) => c is >= '0' and <= '9';

    public static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';

    public static bool IsLowercase(char c) => c is >= 'a' and <= 'z';

    public static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);
}