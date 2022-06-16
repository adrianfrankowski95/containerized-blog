using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Services;

public class LoginService : ILoginService
{
    private readonly UserManager<User> _userManager;
    private readonly IOptionsMonitor<LockoutOptions> _lockoutOptions;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;

    public LoginService(UserManager<User> userManager, IOptionsMonitor<LockoutOptions> lockoutOptions, IOptionsMonitor<EmailOptions> emailOptions)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _lockoutOptions = lockoutOptions ?? throw new ArgumentNullException(nameof(lockoutOptions));
        _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
    }

    public async ValueTask<(IdentityResult result, User? user)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), null);

        User? user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), null);

        if (_userManager.IsLockedOut(user))
            return (IdentityResult.Fail(UserStateValidationError.AccountLockedOut), user);

        var passwordVerificationResult = _userManager.VerifyPassword(user, password);

        if (passwordVerificationResult is PasswordVerificationResult.Success ||
            passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
        {
            if (passwordVerificationResult is PasswordVerificationResult.SuccessNeedsRehash)
                _userManager.UpdatePasswordHash(user, password);


            if (_userManager.IsSuspended(user))
                return (IdentityResult.Fail(UserStateValidationError.AccountSuspended), user);

            var emailOpts = _emailOptions.CurrentValue;

            if (emailOpts.RequireConfirmed && _userManager.IsConfirmingEmail(user))
                return (IdentityResult.Fail(UserStateValidationError.EmailUnconfirmed), user);

            if (_userManager.IsResettingPassword(user))
                return (IdentityResult.Fail(UserStateValidationError.ResettingPassword), user);

            var errors = new List<IdentityError>();

            var userValidationResult = await _userManager.SuccessfulLoginAttemptAsync(user).ConfigureAwait(false);
            if (!userValidationResult.Succeeded)
                errors.AddRange(userValidationResult.Errors);

            var passwordValidationResult = await _userManager.ValidatePasswordAsync(password).ConfigureAwait(false);
            if (!passwordValidationResult.Succeeded)
                errors.AddRange(passwordValidationResult.Errors);

            return errors.Count == 0 ? (IdentityResult.Success, user) : (IdentityResult.Fail(errors), user);
        }
        else if (passwordVerificationResult is PasswordVerificationResult.Fail)
        {
            if (_lockoutOptions.CurrentValue.EnableAccountLockout)
                await _userManager.FailedLoginAttemptAsync(user).ConfigureAwait(false);

            return (IdentityResult.Fail(CredentialsError.InvalidCredentials), user);
        }
        else
            throw new NotSupportedException("Unhandled password verification result");
    }
}
