using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class PasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : UserBase
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
    public async ValueTask ValidateAsync(TUser user, string password, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var passwordHash = user.PasswordHash;

        if (string.IsNullOrWhiteSpace(password) || passwordHash is null || string.IsNullOrWhiteSpace(passwordHash))
        {
            errors.Add(IdentityError.MissingPassword);
            return;
        }

        var opts = _options.CurrentValue;

        //validate password format
        if (password.Length < opts.MinLength ||
            (opts.RequireLowerCase && !password.Any(IsLowercase)) ||
            (opts.RequireUpperCase && !password.Any(IsUppercase)) ||
            (opts.RequireDigit && !password.Any(IsDigit)) ||
            (opts.RequireNonAlphanumeric && !password.Any(IsNonAlphanumeric)))
        {
            errors.Add(IdentityError.InvalidPasswordFormat);
            return;
        }

        var owner = await _userRepository.FindByIdAsync(user.Id).ConfigureAwait(false);

        if (owner is not null)
            if (!owner.PasswordHash.Equals(user.PasswordHash))
            {
                errors.Add(IdentityError.InvalidPassword);
                return;
            }

        //if checked that provided password hash belongs to its owner, verify it
        if (!_passwordHasher.VerifyPassword(password, passwordHash))
            errors.Add(IdentityError.InvalidPassword);
        else if (_passwordHasher.CheckPasswordNeedsRehash(passwordHash))
            errors.Add(IdentityError.PasswordOkNeedsRehash);
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';

    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';

    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);
}