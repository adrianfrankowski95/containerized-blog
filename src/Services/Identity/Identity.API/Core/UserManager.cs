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

    public Task<TUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        var id = GetUserId(principal);

        if (id is null)
            throw new ArgumentNullException(nameof(id));

        return _unitOfWork.Users.FindByIdAsync(id.Value);
    }

    public Guid? GetUserId(ClaimsPrincipal principal)
    {
        if (principal is null)
            throw new ArgumentNullException(nameof(principal));

        string id = principal.FindFirstValue(IdentityConstants.ClaimTypes.Id);

        return Guid.TryParse(id, out Guid userId) ? userId : null;
    }

    public ValueTask<IdentityResult> ValidateUserAsync(TUser user)
        => _userValidator.ValidateAsync(user);

    public ValueTask<IdentityResult> ValidatePasswordAsync(string password)
        => _passwordValidator.ValidateAsync(password);

    public string GetUsername(ClaimsPrincipal principal)
        => principal.FindFirstValue(IdentityConstants.ClaimTypes.Username);

    public async ValueTask<IdentityResult> UpdatePasswordHashAsync(TUser user, string password, bool validatePassword = true)
    {
        ThrowIfNull(user);

        if (validatePassword)
        {
            var result = await ValidatePasswordAsync(password).ConfigureAwait(false);
            if (!result.Succeeded)
                return result;

            //Generate security stamp, verifying password indicates it's a new one
            user.SecurityStamp = GenerateSecurityStamp();
        }

        user.PasswordHash = _passwordHasher.HashPassword(password);
        return IdentityResult.Success;
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

        if (IsLockedOut(user))
            throw new InvalidOperationException("Login attempt by a locked out user");

        var maxAttempts = _securityOptions.CurrentValue.MaxAllowedLoginAttempts;

        user.FailedLoginAttempts = ++user.FailedLoginAttempts;

        if (user.FailedLoginAttempts >= maxAttempts)
            return LockOutAsync(user);
        else
            return UpdateUserAsync(user);
    }

    public bool IsLockedOut(TUser user)
    {
        ThrowIfNull(user);

        return _securityOptions.CurrentValue.EnableAccountLockout &&
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

        if (!_securityOptions.CurrentValue.EnableAccountLockout)
            throw new InvalidOperationException("User lockout is not enabled");

        var lockoutDuration = _securityOptions.CurrentValue.AccountLockoutDuration;

        user.LockedOutUntil = _sysTime.Now.Plus(Duration.FromTimeSpan(lockoutDuration));
        user.FailedLoginAttempts = 0;

        return UpdateUserAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(TUser user, string password)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        user.Role = Role.Reader;
        user.CreatedAt = _sysTime.Now;

        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;
        user.LastLoginAt = null;
        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        user.EmailConfirmed = false;
        user.EmailConfirmationCode = GenerateEmailConfirmationCode();
        user.EmailConfirmationCodeIssuedAt = _sysTime.Now;

        var result = await UpdatePasswordHashAsync(user, password).ConfigureAwait(false);
        if (!result.Succeeded)
            return result;

        return await UpdateUserAsync(user, true).ConfigureAwait(false);
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

    public Task<IdentityResult> UpdateEmailAsync(TUser user, string newEmail)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentNullException(nameof(newEmail));

        user.Email = newEmail;

        user.EmailConfirmed = false;
        user.EmailConfirmationCode = GenerateEmailConfirmationCode();
        user.EmailConfirmationCodeIssuedAt = _sysTime.Now;
        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user, true);
    }

    public Task<IdentityResult> UpdateUsernameAsync(TUser user, string newUsername)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newUsername))
            throw new ArgumentNullException(nameof(newUsername));

        user.Username = newUsername;

        return UpdateUserAsync(user);
    }

    public Task<IdentityResult> ConfirmEmailAsync(TUser user, Guid emailConfirmationCode)
    {
        ThrowIfNull(user);

        if (!IsConfirmingEmail(user))
            return Task.FromResult(IdentityResult.Fail(EmailConfirmationError.EmailAlreadyConfirmed));

        if (emailConfirmationCode == default)
            throw new ArgumentNullException(nameof(emailConfirmationCode));

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

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword));

        if (string.IsNullOrWhiteSpace(oldPassword))
            throw new ArgumentNullException(nameof(oldPassword));

        if (IsResettingPassword(user))
            return IdentityResult.Fail(UserStateValidationError.ResettingPassword);

        var passwordVerificationResult = VerifyPassword(user, oldPassword);

        if (passwordVerificationResult is PasswordVerificationResult.Fail)
            return IdentityResult.Fail(CredentialsError.InvalidCredentials);

        var result = await UpdatePasswordHashAsync(user, newPassword).ConfigureAwait(false);
        if (!result.Succeeded)
            return result;

        return await UpdateUserAsync(user).ConfigureAwait(false);
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

    public Task<IdentityResult> ResetPasswordAsync(TUser user)
    {
        ThrowIfNull(user);

        var length = _securityOptions.CurrentValue.PasswordResetCodeLength;

        user.PasswordResetCode = GeneratePasswordResetCode(length);
        user.PasswordResetCodeIssuedAt = _sysTime.Now;
        user.SecurityStamp = GenerateSecurityStamp();

        return UpdateUserAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(TUser user, string newPassword, string passwordResetCode)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentNullException(nameof(newPassword));

        if (string.IsNullOrWhiteSpace(passwordResetCode))
            throw new ArgumentNullException(nameof(passwordResetCode));

        if (!IsResettingPassword(user))
            return IdentityResult.Fail(PasswordResetError.PasswordResetNotRequested);

        if (IsPasswordResetCodeExpired(user))
            return IdentityResult.Fail(PasswordResetError.ExpiredPasswordResetCode);

        if (!string.Equals(user.PasswordResetCode, passwordResetCode, StringComparison.Ordinal))
            return IdentityResult.Fail(PasswordResetError.InvalidPasswordResetCode);

        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        var result = await UpdatePasswordHashAsync(user, newPassword).ConfigureAwait(false);
        if (!result.Succeeded)
            return result;

        return await UpdateUserAsync(user).ConfigureAwait(false);
    }

    public bool IsPasswordResetCodeExpired(TUser user)
    {
        ThrowIfNull(user);

        if (user.PasswordResetCodeIssuedAt is null)
            throw new ArgumentNullException(nameof(user.PasswordResetCodeIssuedAt));

        var validityPeriod = _securityOptions.CurrentValue.PasswordResetCodeValidityPeriod;

        if (_sysTime.Now > user.PasswordResetCodeIssuedAt.Value.Plus(Duration.FromTimeSpan(validityPeriod)))
            return true;

        return false;
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();
    private static Guid GenerateEmailConfirmationCode() => Guid.NewGuid();

    public async Task<IdentityResult> UpdateUserAsync(TUser user, bool ignoreUnconfirmedEmail = false, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(user);

        var result = await ValidateUserAsync(user).ConfigureAwait(false);

        bool successOrUnconfirmedEmail = result.Succeeded ||
            (ignoreUnconfirmedEmail && result.Errors.Count == 1 &&
            result.Errors.TryGetValue(EmailValidationError.EmailUnconfirmed, out _));

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
