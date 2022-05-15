using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
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
    public ValueTask<IdentityResult> ValidateAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        var opts = _options.CurrentValue;

        var errors = new List<IdentityError>();

        //validate password format
        if (password.Length < opts.MinLength)
            errors.Add(IdentityError.PasswordTooShort);

        if (opts.RequireLowerCase && !password.Any(IsLowercase))
            errors.Add(IdentityError.PasswordWithoutLowerCase);

        if (opts.RequireUpperCase && !password.Any(IsUppercase))
            errors.Add(IdentityError.PasswordWithoutUpperCase);

        if (opts.RequireDigit && !password.Any(IsDigit))
            errors.Add(IdentityError.PasswordWithoutDigit);

        if (opts.RequireNonAlphanumeric && !password.Any(IsNonAlphanumeric))
            errors.Add(IdentityError.PasswordWithoutNonAlphanumeric);

        return
            ValueTask.FromResult(errors.Count == 0 ?
                IdentityResult.Success :
                IdentityResult.Fail(errors));
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsUppercase(char c) => c is >= 'A' and <= 'Z';

    private static bool IsLowercase(char c) => c is >= 'a' and <= 'z';

    private static bool IsNonAlphanumeric(char c) => !IsDigit(c) && !IsUppercase(c) && !IsLowercase(c);
}