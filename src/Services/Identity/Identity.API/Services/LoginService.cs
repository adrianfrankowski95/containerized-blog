using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Services;

public class LoginService<TUser> : ILoginService where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    private readonly IOptionsMonitor<SecurityOptions> _options;

    public LoginService(UserManager<TUser> userManager, IOptionsMonitor<SecurityOptions> options)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<IdentityResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return IdentityResult.Fail(IdentityError.InvalidCredentials);

        var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
            return IdentityResult.Fail(IdentityError.InvalidCredentials);

        var passwordVerificationResult = _userManager.VerifyPassword(user, password);

        if (passwordVerificationResult is PasswordVerificationResult.Success ||
            passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
        {
            if (passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
                await _userManager.UpdatePasswordHashAsync(user, password, false).ConfigureAwait(false);

            var userValidationResult = await _userManager.ValidateUserAsync(user).ConfigureAwait(false);
            if (!userValidationResult.Succeeded)
                return userValidationResult;

            var passwordValidationResult = await _userManager.ValidatePasswordAsync(password).ConfigureAwait(false);
            if (!passwordValidationResult.Succeeded)
                return passwordValidationResult;

            await _userManager.UpdateLastLoginAndClearAttemptsAsync(user).ConfigureAwait(false);
            return IdentityResult.Success;
        }
        else if (passwordVerificationResult is PasswordVerificationResult.Fail)
        {
            if (_options.CurrentValue.EnableLoginAttemptsLock)
            {
                if (_userManager.HasMaxFailedLoginAttempts(user) && !_userManager.IsLocked(user))
                    await _userManager.LockAsync(user).ConfigureAwait(false);
                else
                    await _userManager.AddFailedLoginAttemptAsync(user).ConfigureAwait(false);
            }

            return IdentityResult.Fail(IdentityError.InvalidCredentials);
        }
        else
            throw new NotSupportedException("Unhandled password verification result");
    }
}
