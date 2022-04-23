using System.Security.Claims;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Services;

public class UserManager
{
    private readonly IUserRepository _users;
    private readonly SecurityOptions _options;
    private readonly ISysTime _sysTime;
    private readonly IPasswordHasher _passwordHasher;

    public UserManager(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptionsMonitor<IdentityOptions> options,
        ISysTime sysTime)
    {
        _users = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _options = options.CurrentValue.Security ?? throw new ArgumentNullException(nameof(options));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<IdentityResult> LoginAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email);

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

            await _users.UnitOfWork.CommitChangesAsync().ConfigureAwait(false);
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

        await _users.UnitOfWork.CommitChangesAsync().ConfigureAwait(false);

        return IdentityResult.Success;
    }



    public async Task<IdentityResult> VerifyCredentialsAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email);

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

        var currentStamp = await _users.GetUserSecurityStampAsync(userGuid).ConfigureAwait(false);

        if (!securityStamp.Equals(currentStamp))
            return IdentityResult.Fail(IdentityError.InvalidOrMissingSecurityStamp);

        return IdentityResult.Success;
    }
    private static void SetNewLastLoginTime(User user, ISysTime sysTime)
    {
        user.LastLoginAt = sysTime.Now;
    }

    private static void SetNewSecurityStamp(User user)
    {
        user.SecurityStamp = GenerateSecurityStamp();
    }

    private static void AddFailedLoginAttempt(User user)
    {
        user.FailedLoginAttempts = ++user.FailedLoginAttempts;
    }

    //true if change was made, false if no change was needed
    private static void ClearFailedLoginAttempts(User user)
    {
        if (user.FailedLoginAttempts == 0)
            return;

        user.FailedLoginAttempts = new(0);
    }

    private static void LockUser(User user, int durationMinutes, ISysTime sysTime)
    {
        if (user.IsCurrentlyLocked)
            throw new InvalidOperationException("User is already locked");

        user.LockedUntil = sysTime.Now.Plus(Duration.FromMinutes(durationMinutes));
    }

    //true if change was made, false if no change was needed
    private static bool ClearLock(User user)
    {
        if (!user.LockExists)
            return false;

        user.LockedUntil = null;
        return true;
    }

    private static string GeneratePasswordResetCode(int length, ISysTime sysTime)
    {
        var rand = new Random((int)sysTime.Now.ToUnixTimeTicks());

        byte[] randomBytes = new byte[length * 2]; //1 string takes 2 bytes
        rand.NextBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();
}
