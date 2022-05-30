using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class PasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : User
{
    private readonly IOptionsMonitor<PasswordOptions> _options;

    public PasswordValidator(IOptionsMonitor<PasswordOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    public ValueTask<IdentityResult> ValidateAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ValueTask.FromResult(IdentityResult.Fail(PasswordValidationError.MissingPassword));
        }

        var opts = _options.CurrentValue;
        var errors = new List<IdentityError>();

        //validate password format
        if (password.Length < opts.MinLength)
            errors.Add(PasswordValidationError.PasswordTooShort);

        if (opts.RequireLowerCase && !password.Any(IsLowercase))
            errors.Add(PasswordValidationError.PasswordWithoutLowerCase);

        if (opts.RequireUpperCase && !password.Any(IsUppercase))
            errors.Add(PasswordValidationError.PasswordWithoutUpperCase);

        if (opts.RequireDigit && !password.Any(IsDigit))
            errors.Add(PasswordValidationError.PasswordWithoutDigit);

        if (opts.RequireNonAlphanumeric && !password.Any(IsNonAlphanumeric))
            errors.Add(PasswordValidationError.PasswordWithoutNonAlphanumeric);

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