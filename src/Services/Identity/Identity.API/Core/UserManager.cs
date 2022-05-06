using System.Security.Claims;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class UserManager<TUser> where TUser : UserBase
{
    private readonly IUnitOfWork<TUser> _unitOfWork;
    private readonly IOptionsMonitor<SecurityOptions> _options;
    private readonly ISysTime _sysTime;
    private readonly IPasswordHasher _passwordHasher;

    public UserManager(
        IUnitOfWork<TUser> unitOfWork,
        IPasswordHasher passwordHasher,
        IOptionsMonitor<SecurityOptions> options,
        ISysTime sysTime)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<IdentityResult> LoginAsync(string email, string password)
    {
        var user = await _unitOfWork.Users.FindByEmailAsync(email);

        if (user is null)
            return IdentityResult.Fail(IdentityError.InvalidCredentials);

        if (user.IsCurrentlyLocked)
            return IdentityResult.Fail(IdentityError.AccountLocked);

        //if user exists and provided wrong password - add a failed login attempt
        //or lock if maximum allowed login attempts are exceeded
        //then return invalid credentials error
        if (!VerifyPassword(password, user.PasswordHash.ToString()))
        {
            if (user.FailedLoginAttempts >= _options.MaxAllowedLoginAttempts)
                LockUser(user, _options.AccountLockDurationMinutes, _sysTime);

            else
                AddFailedLoginAttempt(user);

            await _unitOfWork.UnitOfWork.CommitChangesAsync().ConfigureAwait(false);
            return IdentityResult.Fail(IdentityError.InvalidCredentials);
        }

        if (user.IsCurrentlySuspended)
            return IdentityResult.Fail(IdentityError.AccountSuspended);

        if (!user.EmailConfirmed)
            return IdentityResult.Fail(IdentityError.UnconfirmedEmail);

        if (user.IsResettingPassword)
            return IdentityResult.Fail(IdentityError.ResettingPassword);

        //validation sucessful
        ClearFailedLoginAttempts(user);
        SetNewLastLoginTime(user, _sysTime);

        await _unitOfWork.CommitChangesAsync().ConfigureAwait(false);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> VerifyCredentialsAsync(string email, string password)
    {
        var user = await _unitOfWork.Users.FindByEmailAsync(email);

        if (user is null || !VerifyPassword(password, user.PasswordHash.ToString()))
            return IdentityResult.Fail(IdentityError.InvalidCredentials);

        return IdentityResult.Success;
    }

    public bool VerifyPassword(string password, string passwordHash)
        => _passwordHasher.VerifyPassword(password, passwordHash);

    public string HashPassword(string password) => _passwordHasher.HashPassword(password);

    public async Task<IdentityResult> VerfifySecurityStamp(ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirstValue(IdentityConstants.ClaimTypes.Id);

        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out Guid userGuid))
            return IdentityResult.Fail(IdentityError.InvalidIdentifier);

        var securityStamp = claimsPrincipal.FindFirstValue(IdentityConstants.ClaimTypes.SecurityStamp);

        if (string.IsNullOrWhiteSpace(securityStamp) || !Guid.TryParse(securityStamp, out Guid securityStampGuid) ||
            securityStampGuid.Equals(default))
            return IdentityResult.Fail(IdentityError.InvalidOrMissingSecurityStamp);

        var currentStamp = await _unitOfWork.GetUserSecurityStampAsync(userGuid).ConfigureAwait(false);

        if (!securityStamp.Equals(currentStamp))
            return IdentityResult.Fail(IdentityError.InvalidOrMissingSecurityStamp);

        return IdentityResult.Success;
    }
    public void SetLastLoginAt(TUser user)
    {
        user.LastLoginAt = _sysTime.Now;
    }

    public static void AddFailedLoginAttempt(TUser user)
    {
        user.FailedLoginAttempts = ++user.FailedLoginAttempts;
    }

    public static void ClearFailedLoginAttempts(TUser user)
    {
        if (user.FailedLoginAttempts == 0)
            return;

        user.FailedLoginAttempts = 0;
    }

    public void LockUser(TUser user)
    {
        var lockDuration = _options.CurrentValue.AccountLockDurationMinutes;

        if (IsLocked(user))
            throw new InvalidOperationException("User is already locked");

        user.LockedUntil = _sysTime.Now.Plus(Duration.FromMinutes(lockDuration));
    }

    public bool IsLocked(TUser user) => user.LockedUntil is not null && user.LockedUntil > _sysTime.Now;

    public static void ClearLock(TUser user) => user.LockedUntil = null;

    private string GeneratePasswordResetCode(int length)
    {
        var allowedChars = _options.CurrentValue.PasswordResetCodeAllowedCharacters;

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
    public void SetPasswordResetCode(TUser user)
    {
        var length = _options.CurrentValue.PasswordResetCodeLength;
        user.PasswordResetCode = GeneratePasswordResetCode(length);
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();
    public static void SetSecurityStamp(TUser user)
         => user.SecurityStamp = GenerateSecurityStamp();

    public async Task CommitChangesAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.CommitChangesAsync(cancellationToken);
}
