using System.Security.Claims;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class UserManager<TUser> where TUser : User
{
    private readonly IUnitOfWork<TUser> _unitOfWork;
    private readonly IOptionsMonitor<PasswordOptions> _passwordOptions;
    private readonly IOptionsMonitor<LockoutOptions> _lockoutOptions;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly ISysTime _sysTime;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserValidator<TUser> _userValidator;
    private readonly IPasswordValidator<TUser> _passwordValidator;

    public UserManager(
        IUnitOfWork<TUser> unitOfWork,
        IPasswordHasher passwordHasher,
        IOptionsMonitor<PasswordOptions> passwordOptions,
        IOptionsMonitor<LockoutOptions> lockoutOptions,
        IOptionsMonitor<EmailOptions> emailOptions,
        IUserValidator<TUser> userValidator,
        IPasswordValidator<TUser> passwordValidator,
        ISysTime sysTime)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordOptions = passwordOptions ?? throw new ArgumentNullException(nameof(passwordOptions));
        _lockoutOptions = lockoutOptions ?? throw new ArgumentNullException(nameof(lockoutOptions));
        _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
    }

    public Task<TUser?> FindByEmailAsync(string email)
        => _unitOfWork.Users.FindByEmailAsync(email);

    public Task<TUser?> FindByIdAsync(Guid userId)
        => _unitOfWork.Users.FindByIdAsync(userId);

    public Task<TUser?> GetUserAsync(ClaimsPrincipal? principal)
    {
        var id = GetUserId(principal);

        if (id is null)
            return Task.FromResult<TUser?>(null);

        return _unitOfWork.Users.FindByIdAsync(id.Value);
    }

    public Guid? GetUserId(ClaimsPrincipal? principal)
    {
        if (principal is null)
            return null;

        string id = principal.FindFirstValue(IdentityConstants.ClaimTypes.Id);

        return Guid.TryParse(id, out Guid userId) ? userId : null;
    }

    public ValueTask<IdentityResult> ValidateUserAsync(TUser user)
        => _userValidator.ValidateAsync(user);

    public ValueTask<IdentityResult> ValidatePasswordAsync(string password)
        => _passwordValidator.ValidateAsync(password);

    public void UpdatePasswordHash(TUser user, string password)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        //If user has no password hash yet or
        //provided password is different, generate new security stamp
        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            VerifyPassword(user, password) is PasswordVerificationResult.Fail)
        {
            user.SecurityStamp = GenerateSecurityStamp();
        }

        user.PasswordHash = _passwordHasher.HashPassword(password);
    }

    public PasswordVerificationResult VerifyPassword(TUser user, string password)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public bool VerifyPasswordResetCode(TUser user, string? passwordResetCode)
        => !string.IsNullOrWhiteSpace(passwordResetCode) &&
            string.Equals(user.PasswordResetCode, passwordResetCode, StringComparison.Ordinal);

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

        if (IsLockedOut(user))
            throw new InvalidOperationException("Login attempt by a locked out user");

        var maxAttempts = _lockoutOptions.CurrentValue.MaxAllowedLoginAttempts;

        user.FailedLoginAttempts = ++user.FailedLoginAttempts;

        if (user.FailedLoginAttempts >= maxAttempts)
            return LockOutAsync(user);
        else
            return UpdateUserAsync(user);
    }

    public bool IsLockedOut(TUser user)
    {
        ThrowIfNull(user);

        return _lockoutOptions.CurrentValue.EnableAccountLockout &&
            user.LockedOutUntil is not null && user.LockedOutUntil > _sysTime.Now;
    }

    public bool IsSuspended(TUser user)
    {
        ThrowIfNull(user);
        return user.SuspendedUntil is not null && user.SuspendedUntil > _sysTime.Now;
    }

    public bool IsResettingPassword(TUser user)
    {
        ThrowIfNull(user);

        bool passwordResetCodeExists = user.PasswordResetCode is not null;
        bool issuedAtExists = user.PasswordResetCodeIssuedAt is not null;

        if (passwordResetCodeExists && !issuedAtExists)
            throw new InvalidOperationException("Password reset code exists, but its creation date is not set");

        if (!passwordResetCodeExists && issuedAtExists)
            throw new InvalidOperationException("Password reset code creation date exists, but code has not been generated");

        return passwordResetCodeExists && issuedAtExists;
    }

    private Task<IdentityResult> LockOutAsync(TUser user)
    {
        ThrowIfNull(user);

        if (IsLockedOut(user))
            throw new InvalidOperationException("User is already locked out");

        if (!_lockoutOptions.CurrentValue.EnableAccountLockout)
            throw new InvalidOperationException("User lockout is not enabled");

        var lockoutDuration = _lockoutOptions.CurrentValue.AccountLockoutDuration;

        user.LockedOutUntil = _sysTime.Now.Plus(Duration.FromTimeSpan(lockoutDuration));
        user.FailedLoginAttempts = 0;

        return UpdateUserAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(TUser user, string password)
    {
        ThrowIfNull(user);

        var passwordValidationResult = await ValidatePasswordAsync(password).ConfigureAwait(false);
        if (!passwordValidationResult.Succeeded)
            return passwordValidationResult;

        user.Role = Role.Reader;
        user.CreatedAt = _sysTime.Now;

        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;
        user.LastLoginAt = null;
        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        UpdatePasswordHash(user, password);

        return await GenerateEmailConfirmationAsync(user).ConfigureAwait(false);
    }

    public Task<IdentityResult> UpdateRoleAsync(TUser user, Role role)
    {
        ThrowIfNull(user);

        if (role is null)
            throw new ArgumentNullException(nameof(role));

        if (user.Role.Equals(role))
            throw new InvalidOperationException("User is already in a provided role");

        user.Role = role;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> UpdateEmailAsync(TUser user, string newEmail)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newEmail))
            return Task.FromResult(IdentityResult.Fail(EmailValidationError.MissingEmail));

        if (string.Equals(user.EmailAddress, newEmail, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(IdentityResult.Fail(EmailValidationError.NewAndOldEmailsAreEqual));

        user.EmailAddress = newEmail;

        return GenerateEmailConfirmationAsync(user);
    }

    public Task<IdentityResult> UpdateUsernameAsync(TUser user, string newUsername)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newUsername))
            throw new ArgumentNullException(nameof(newUsername));

        if (string.Equals(user.Username, newUsername, StringComparison.Ordinal))
            return Task.FromResult(IdentityResult.Fail(UsernameValidationError.NewAndOldUsernamesAreEqual));

        user.Username = newUsername;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> ConfirmEmailAsync(TUser user, Guid emailConfirmationCode)
    {
        ThrowIfNull(user);

        if (!IsConfirmingEmail(user))
            return Task.FromResult(IdentityResult.Fail(EmailConfirmationError.InvalidEmailConfirmationCode));

        if (emailConfirmationCode == default)
            return Task.FromResult(IdentityResult.Fail(EmailConfirmationError.InvalidEmailConfirmationCode));

        if (!user.EmailConfirmationCode.Equals(emailConfirmationCode))
            return Task.FromResult(IdentityResult.Fail(EmailConfirmationError.InvalidEmailConfirmationCode));

        if (IsEmailConfirmationCodeExpired(user))
            return Task.FromResult(IdentityResult.Fail(EmailConfirmationError.ExpiredEmailConfirmationCode));

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeIssuedAt = null;

        return UpdateUserAsync(user);
    }

    public bool IsConfirmingEmail(TUser user)
    {
        ThrowIfNull(user);

        bool emailConfirmed = user.EmailConfirmed;
        bool confirmationCodeExists = user.EmailConfirmationCode is not null && user.EmailConfirmationCode.Value != default;
        bool issuedAtExists = user.EmailConfirmationCodeIssuedAt is not null;

        if (emailConfirmed)
        {
            if (confirmationCodeExists)
                throw new InvalidOperationException("Email address is confirmed, but confirmation code exists");

            if (issuedAtExists)
                throw new InvalidOperationException("Email address is confirmed, but confirmation code creation date exists");
        }
        else
        {
            if (!confirmationCodeExists)
                throw new InvalidOperationException("Email address is not confirmed, but confirmation code has not been generated");

            if (!issuedAtExists)
                throw new InvalidOperationException("Email confiration code exists, but its creation date has not been set");
        }

        return !emailConfirmed;
    }

    public Task<IdentityResult> GenerateEmailConfirmationAsync(TUser user)
    {
        user.EmailConfirmed = false;
        user.EmailConfirmationCode = GenerateEmailConfirmationCode();
        user.EmailConfirmationCodeIssuedAt = _sysTime.Now;

        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user);
    }

    public bool IsEmailConfirmationCodeExpired(TUser user)
    {
        if (user.EmailConfirmationCodeIssuedAt is null)
            throw new ArgumentNullException(nameof(user.EmailConfirmationCodeIssuedAt));

        var validityPeriod = _emailOptions.CurrentValue.EmailConfirmationCodeValidityPeriod;

        if (_sysTime.Now > user.EmailConfirmationCodeIssuedAt.Value.Plus(Duration.FromTimeSpan(validityPeriod)))
            return true;

        return false;
    }

    public async Task<IdentityResult> UpdatePasswordAsync(TUser user, string newPassword, string oldPassword)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(oldPassword))
            return IdentityResult.Fail(PasswordValidationError.MissingPassword);

        if (string.Equals(newPassword, oldPassword, StringComparison.Ordinal))
            return IdentityResult.Fail(PasswordValidationError.NewAndOldPasswordsAreEqual);

        var passwordValidationResult = await ValidatePasswordAsync(newPassword).ConfigureAwait(false);
        if (!passwordValidationResult.Succeeded)
            return passwordValidationResult;

        var passwordVerificationResult = VerifyPassword(user, oldPassword);
        if (passwordVerificationResult is PasswordVerificationResult.Fail)
            return IdentityResult.Fail(CredentialsError.InvalidCredentials);

        UpdatePasswordHash(user, newPassword);

        return await UpdateUserAsync(user).ConfigureAwait(false);
    }

    private string GeneratePasswordResetCode(int length)
    {
        var allowedChars = _passwordOptions.CurrentValue.PasswordResetCodeAllowedCharacters;

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

    public Task<IdentityResult> ResetPasswordAsync(TUser user)
    {
        ThrowIfNull(user);

        var length = _passwordOptions.CurrentValue.PasswordResetCodeLength;

        user.PasswordResetCode = GeneratePasswordResetCode(length);
        user.PasswordResetCodeIssuedAt = _sysTime.Now;
        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(TUser user, string newPassword, string passwordResetCode)
    {
        ThrowIfNull(user);

        if (!IsResettingPassword(user))
            return IdentityResult.Fail(PasswordResetError.InvalidPasswordResetCode);

        if (IsPasswordResetCodeExpired(user))
            return IdentityResult.Fail(PasswordResetError.ExpiredPasswordResetCode);

        if (!VerifyPasswordResetCode(user, passwordResetCode))
            return IdentityResult.Fail(PasswordResetError.InvalidPasswordResetCode);

        var passwordValidationResult = await ValidatePasswordAsync(newPassword).ConfigureAwait(false);
        if (!passwordValidationResult.Succeeded)
            return passwordValidationResult;

        var passwordVerificationResult = VerifyPassword(user, newPassword);
        if (passwordVerificationResult is not PasswordVerificationResult.Fail)
            return IdentityResult.Fail(PasswordResetError.NewAndOldPasswordsAreEqual);

        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        UpdatePasswordHash(user, newPassword);

        return await UpdateUserAsync(user).ConfigureAwait(false);
    }

    public bool IsPasswordResetCodeExpired(TUser user)
    {
        ThrowIfNull(user);

        if (user.PasswordResetCodeIssuedAt is null)
            throw new ArgumentNullException(nameof(user.PasswordResetCodeIssuedAt));

        var validityPeriod = _passwordOptions.CurrentValue.PasswordResetCodeValidityPeriod;

        if (_sysTime.Now > user.PasswordResetCodeIssuedAt.Value.Plus(Duration.FromTimeSpan(validityPeriod)))
            return true;

        return false;
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();
    private static Guid GenerateEmailConfirmationCode() => Guid.NewGuid();

    public async Task<IdentityResult> UpdateUserAsync(
        TUser user,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNull(user);

        var result = await ValidateUserAsync(user).ConfigureAwait(false);

        if (!result.Succeeded)
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

    private static void ThrowIfNull(TUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));
    }
}
