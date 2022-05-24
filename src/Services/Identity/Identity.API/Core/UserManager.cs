using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class UserManager<TUser> where TUser : User
{
    private readonly IUnitOfWork<TUser> _unitOfWork;
    private readonly IOptionsMonitor<SecurityOptions> _securityOptions;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly ISysTime _sysTime;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserValidator<TUser> _userValidator;
    private readonly IPasswordValidator<TUser> _passwordValidator;
    private readonly IUserClaimsPrincipalFactory<TUser> _claimsPrincipalFactory;

    public UserManager(
        IUnitOfWork<TUser> unitOfWork,
        IPasswordHasher passwordHasher,
        IOptionsMonitor<SecurityOptions> securityOptions,
        IOptionsMonitor<EmailOptions> emailOptions,
        IUserValidator<TUser> userValidator,
        IPasswordValidator<TUser> passwordValidator,
        IUserClaimsPrincipalFactory<TUser> claimsPrincipalFactory,
        ISysTime sysTime)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _securityOptions = securityOptions ?? throw new ArgumentNullException(nameof(securityOptions));
        _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
    }

    public Task<TUser?> FindByEmailAsync(string email)
        => _unitOfWork.Users.FindByEmailAsync(email);

    public Task<TUser?> FindByIdAsync(Guid userId)
        => _unitOfWork.Users.FindByIdAsync(userId);

    public ValueTask<IdentityResult> ValidateUserAsync(TUser user)
        => _userValidator.ValidateAsync(user);

    public ValueTask<IdentityResult> ValidatePasswordAsync(string password)
        => _passwordValidator.ValidateAsync(password);

    public string GetUsername(ClaimsPrincipal principal)
        => principal.FindFirstValue(IdentityConstants.ClaimTypes.Username);

    public async Task<IdentityResult> UpdatePasswordHashAsync(TUser user, string password, bool validatePassword = true)
    {
        ThrowIfNull(user);

        if (validatePassword)
        {
            var result = await ValidatePasswordAsync(password).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            user.SecurityStamp = GenerateSecurityStamp(); //generate security stamp, verifying password means it's a new one
        }

        user.PasswordHash = _passwordHasher.HashPassword(password);
        return await UpdateUserAsync(user).ConfigureAwait(false);
    }

    public PasswordVerificationResult VerifyPassword(TUser user, string password)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            return PasswordVerificationResult.Fail;

        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public Task<IdentityResult> SuccessfulLoginAttemptAsync(TUser user)
    {
        ThrowIfNull(user);

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = _sysTime.Now;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> SuspendAsync(TUser user, Instant suspendUntil)
    {
        ThrowIfNull(user);

        if (suspendUntil < _sysTime.Now)
            throw new InvalidOperationException("Suspended until date must not be in the past");

        user.SuspendedUntil = suspendUntil;
        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> FailedLoginAttemptAsync(TUser user)
    {
        ThrowIfNull(user);

        if (IsLocked(user))
            throw new InvalidOperationException("Login attempt for a locked user");

        var maxAttempts = _securityOptions.CurrentValue.MaxAllowedLoginAttempts;

        user.FailedLoginAttempts = ++user.FailedLoginAttempts;

        if (user.FailedLoginAttempts >= maxAttempts)
            return LockAsync(user);
        else
            return UpdateUserAsync(user);
    }

    public bool IsLocked(TUser user)
    {
        ThrowIfNull(user);

        return _securityOptions.CurrentValue.EnableLoginAttemptsLock &&
            user.LockedUntil is not null && user.LockedUntil > _sysTime.Now;
    }

    public bool IsSuspended(TUser user)
    {
        ThrowIfNull(user);
        return user.SuspendedUntil is not null && user.SuspendedUntil > _sysTime.Now;
    }

    public bool IsResettingPassword(TUser user)
    {
        ThrowIfNull(user);
        return user.PasswordResetCode is not null;
    }

    private Task<IdentityResult> LockAsync(TUser user)
    {
        ThrowIfNull(user);

        if (IsLocked(user))
            throw new InvalidOperationException("User is already locked");

        if (!_securityOptions.CurrentValue.EnableLoginAttemptsLock)
            throw new InvalidOperationException("User lock is not enabled");

        var lockDuration = _securityOptions.CurrentValue.AccountLockDuration;

        user.LockedUntil = _sysTime.Now.Plus(Duration.FromTimeSpan(lockDuration));
        user.FailedLoginAttempts = 0;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> CreateUserAsync(TUser user, string password, out Guid emailConfirmationCode)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        user.Role = Role.Reader;
        user.CreatedAt = _sysTime.Now;

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = null;
        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        emailConfirmationCode = GenerateEmailConfirmationCode();

        user.EmailConfirmed = false;
        user.EmailConfirmationCode = emailConfirmationCode;
        user.EmailConfirmationCodeIssuedAt = _sysTime.Now;

        return UpdatePasswordHashAsync(user, password);
    }

    public Task<IdentityResult> UpdateRoleAsync(TUser user, Role role)
    {
        ThrowIfNull(user);

        if (role is null)
            throw new ArgumentNullException(nameof(role));

        if (user.Role.Equals(role))
            throw new InvalidOperationException("User already is in a provided role");

        user.Role = role;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> UpdateEmailAsync(TUser user, string newEmail, out Guid emailConfirmationCode)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentNullException(nameof(newEmail));

        user.Email = newEmail;
        user.EmailConfirmed = false;

        emailConfirmationCode = GenerateEmailConfirmationCode();

        user.EmailConfirmationCode = emailConfirmationCode;
        user.EmailConfirmationCodeIssuedAt = _sysTime.Now;

        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user, true);
    }

    public Task<IdentityResult> ConfirmEmailAsync(TUser user, Guid emailConfirmationCode)
    {
        ThrowIfNull(user);

        if (user.EmailConfirmed)
            return Task.FromResult(IdentityResult.Fail(IdentityError.EmailAlreadyConfirmed));

        if (emailConfirmationCode == default)
            throw new ArgumentNullException(nameof(emailConfirmationCode));

        if (user.EmailConfirmationCode is null)
            return Task.FromResult(IdentityResult.Fail(IdentityError.MissingEmailConfirmationCode));

        if (!user.EmailConfirmationCode.Equals(emailConfirmationCode))
            return Task.FromResult(IdentityResult.Fail(IdentityError.InvalidEmailConfirmationCode));

        if (user.EmailConfirmationCodeIssuedAt is null)
            throw new ArgumentNullException(nameof(user.EmailConfirmationCodeIssuedAt));

        var validityPeriod = _emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod;

        if (_sysTime.Now > user.EmailConfirmationCodeIssuedAt.Value.Plus(Duration.FromTimeSpan(validityPeriod)))
            return Task.FromResult(IdentityResult.Fail(IdentityError.ExpiredEmailConfirmationCode));

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeIssuedAt = null;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> ResetPasswordAsync(TUser user, string newPassword, string passwordResetCode)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword));

        if (string.IsNullOrWhiteSpace(passwordResetCode))
            throw new ArgumentNullException(nameof(passwordResetCode));

        if (user.PasswordResetCode is null)
            return Task.FromResult(IdentityResult.Fail(IdentityError.MissingPasswordResetCode));

        if (!string.Equals(user.PasswordResetCode, passwordResetCode, StringComparison.Ordinal))
            return Task.FromResult(IdentityResult.Fail(IdentityError.InvalidPasswordResetCode));

        if (user.PasswordResetCodeIssuedAt is null)
            throw new ArgumentNullException(nameof(user.PasswordResetCodeIssuedAt));

        var validityPeriod = _securityOptions.CurrentValue.PasswordResetCodeValidityPeriod;

        if (_sysTime.Now > user.PasswordResetCodeIssuedAt.Value.Plus(Duration.FromTimeSpan(validityPeriod)))
            return Task.FromResult(IdentityResult.Fail(IdentityError.ExpiredEmailConfirmationCode));

        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        return UpdatePasswordHashAsync(user, newPassword);
    }

    public Task<IdentityResult> UpdatePasswordAsync(TUser user, string newPassword, string oldPassword)
    {
        ThrowIfNull(user);

        if (IsResettingPassword(user))
            throw new InvalidOperationException("User cannot change his password during resetting");

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword));

        if (string.IsNullOrWhiteSpace(oldPassword))
            throw new ArgumentNullException(nameof(oldPassword));

        var passwordVerificationResult = VerifyPassword(user, oldPassword);

        if (passwordVerificationResult is PasswordVerificationResult.Fail)
            return Task.FromResult(IdentityResult.Fail(IdentityError.InvalidCredentials));

        return UpdatePasswordHashAsync(user, newPassword);
    }

    private string GeneratePasswordResetCode(int length)
    {
        var allowedChars = _securityOptions.CurrentValue.PasswordResetCodeAllowedCharacters;

        if (string.IsNullOrEmpty(allowedChars))
            throw new ArgumentNullException(nameof(allowedChars));

        var rnd = Random.Shared;
        var code = new char[length];

        for (int i = 0; i < length; ++i)
        {
            code[i] = allowedChars[rnd.Next(0, allowedChars.Length - 1)];
        }

        return new string(code);
    }

    public Task<IdentityResult> ResetPasswordAsync(TUser user, out string passwordResetCode)
    {
        ThrowIfNull(user);

        var length = _securityOptions.CurrentValue.PasswordResetCodeLength;

        passwordResetCode = GeneratePasswordResetCode(length);

        user.PasswordResetCode = passwordResetCode;
        user.PasswordResetCodeIssuedAt = _sysTime.Now;
        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user);
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();
    private static Guid GenerateEmailConfirmationCode() => Guid.NewGuid();

    public async Task<IdentityResult> UpdateUserAsync(TUser user, bool ignoreUnconfirmedEmail = false, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(user);

        var result = await ValidateUserAsync(user).ConfigureAwait(false);

        bool successOrUnconfirmedEmail = result.Succeeded ||
            (ignoreUnconfirmedEmail && result.Errors.Count() == 1 &&
            result.Errors.Single().Equals(IdentityError.EmailUnconfirmed));

        if (!successOrUnconfirmedEmail)
            return result;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CommitChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public Task RemoveUserAsync(TUser user, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Users.Remove(user);
        return _unitOfWork.CommitChangesAsync(cancellationToken);
    }

    public ValueTask<ClaimsPrincipal> CreateUserPrincipalAsync(TUser user)
        => _claimsPrincipalFactory.CreateAsync(user);

    private static void ThrowIfNull(TUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));
    }
}
