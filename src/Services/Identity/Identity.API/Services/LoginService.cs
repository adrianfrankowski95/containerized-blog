using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Services;

public class LoginService<TUser> : ILoginService<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    private readonly IOptionsMonitor<SecurityOptions> _options;

    public LoginService(UserManager<TUser> userManager, IOptionsMonitor<SecurityOptions> options)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<(IdentityResult result, TUser? user)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), null);

        TUser? user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), null);

        var passwordVerificationResult = _userManager.VerifyPassword(user, password);

        if (passwordVerificationResult is PasswordVerificationResult.Success ||
            passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
        {
            var userValidationResult = await _userManager.ValidateUserAsync(user).ConfigureAwait(false);
            if (!userValidationResult.Succeeded)
                return (userValidationResult, null);

            var passwordValidationResult = await _userManager.ValidatePasswordAsync(password).ConfigureAwait(false);
            if (!passwordValidationResult.Succeeded)
                return (passwordValidationResult, null);

            if (passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
            {
                var passwordHashUpdateResult = await _userManager.UpdatePasswordHashAsync(user, password, false).ConfigureAwait(false);
                if (!passwordHashUpdateResult.Succeeded)
                    return (passwordHashUpdateResult, null);
            }

            var result = await _userManager.SuccessfulLoginAttemptAsync(user).ConfigureAwait(false);
            return (result, result.Succeeded ? user : null);
        }
        else if (passwordVerificationResult is PasswordVerificationResult.Fail)
        {
            if (_options.CurrentValue.EnableAccountLockout)
                await _userManager.FailedLoginAttemptAsync(user).ConfigureAwait(false);

            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), null);
        }
        else
            throw new NotSupportedException("Unhandled password verification result");
    }
}
