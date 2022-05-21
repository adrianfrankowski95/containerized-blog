using System.Security.Claims;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class UserManager<TUser> where TUser : User
{
    private readonly IUnitOfWork<TUser> _unitOfWork;
    private readonly IOptionsMonitor<SecurityOptions> _options;
    private readonly ISysTime _sysTime;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserValidator<TUser> _userValidator;
    private readonly IPasswordValidator<TUser> _passwordValidator;
    private readonly IUserClaimsPrincipalFactory<TUser> _claimsPrincipalFactory;

    public UserManager(
        IUnitOfWork<TUser> unitOfWork,
        IPasswordHasher passwordHasher,
        IOptionsMonitor<SecurityOptions> options,
        IUserValidator<TUser> userValidator,
        IPasswordValidator<TUser> passwordValidator,
        IUserClaimsPrincipalFactory<TUser> claimsPrincipalFactory,
        ISysTime sysTime)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
    }

    public Task<TUser?> FindByEmailAsync(string email)
        => _unitOfWork.Users.FindByEmailAsync(email);

    public ValueTask<IdentityResult> ValidateUserAsync(TUser user)
        => _userValidator.ValidateAsync(user);

    public ValueTask<IdentityResult> ValidatePasswordAsync(string password)
        => _passwordValidator.ValidateAsync(password);

    public async ValueTask<IdentityResult> UpdatePasswordHashAsync(TUser user, string password, bool validatePassword = true)
    {
        ThrowIfNull(user);
        IdentityResult result = null!;

        if (validatePassword)
            result = await ValidatePasswordAsync(password);

        if (!validatePassword || result.Equals(IdentityResult.Success))
        {
            user.PasswordHash = _passwordHasher.HashPassword(password);
            await UpdateUserAsync(user).ConfigureAwait(false);
        }

        return validatePassword ? result : IdentityResult.Success;
    }

    public PasswordVerificationResult VerifyPassword(TUser user, string password)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(password))
            return PasswordVerificationResult.Fail;

        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public Task UpdateLastLoginAndClearAttemptsAsync(TUser user)
    {
        ThrowIfNull(user);

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = _sysTime.Now;

        return UpdateUserAsync(user);
    }

    public Task AddFailedLoginAttemptAsync(TUser user)
    {
        ThrowIfNull(user);

        var maxAttempts = _options.CurrentValue.MaxAllowedLoginAttempts;

        if (user.FailedLoginAttempts >= maxAttempts)
            throw new InvalidOperationException("User already has maximum number of failed attempts");

        user.FailedLoginAttempts = ++user.FailedLoginAttempts;
        return UpdateUserAsync(user);
    }

    public bool HasMaxFailedLoginAttempts(TUser user)
    {
        ThrowIfNull(user);

        var maxAttempts = _options.CurrentValue.MaxAllowedLoginAttempts;

        return user.FailedLoginAttempts >= maxAttempts;
    }

    public bool IsLocked(TUser user)
    {
        ThrowIfNull(user);
        return user.LockedUntil is not null && user.LockedUntil > _sysTime.Now;
    }

    public Task LockAsync(TUser user)
    {
        ThrowIfNull(user);

        if (IsLocked(user))
            throw new InvalidOperationException("User is already locked");

        var lockDuration = _options.CurrentValue.AccountLockDurationMinutes;

        user.LockedUntil = _sysTime.Now.Plus(Duration.FromMinutes(lockDuration));
        return UpdateUserAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(TUser user)
    {
        ThrowIfNull(user);

        user.SecurityStamp = GenerateSecurityStamp();
        user.Role = Role.Reader;
        user.CreatedAt = _sysTime.Now;
        user.FailedLoginAttempts = 0;
        user.EmailConfirmed = false;
        user.LockedUntil = null;
        user.LastLoginAt = null;
        user.PasswordResetCode = null;
        user.PasswordResetCodeIssuedAt = null;

        var result = await ValidateUserAsync(user).ConfigureAwait(false);

        bool successOrEmailUnconfirmed = result.Succeeded ||
            (result.Errors.Count() == 1 && result.Errors.Single().Equals(IdentityError.EmailUnconfirmed));

        if (successOrEmailUnconfirmed)
            await AddUserAsync(user).ConfigureAwait(false);

        return successOrEmailUnconfirmed ? IdentityResult.Success : result;
    }

    public async Task<IdentityResult> UpdateEmailAsync(TUser user, string newEmail)
    {
        ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentNullException(nameof(newEmail));

        user.Email = newEmail;
        user.EmailConfirmed = false;
        user.SecurityStamp = GenerateSecurityStamp();

        var result = await ValidateUserAsync(user).ConfigureAwait(false);

        bool successOrEmailUnconfirmed = result.Succeeded ||
            (result.Errors.Count() == 1 && result.Errors.Single().Equals(IdentityError.EmailUnconfirmed));

        if (successOrEmailUnconfirmed)
            await UpdateUserAsync(user).ConfigureAwait(false);

        return successOrEmailUnconfirmed ? IdentityResult.Success : result;
    }

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

    public async Task<string> ResetPasswordAsync(TUser user)
    {
        ThrowIfNull(user);

        var length = _options.CurrentValue.PasswordResetCodeLength;

        var code = GeneratePasswordResetCode(length);

        user.PasswordResetCode = code;
        user.PasswordResetCodeIssuedAt = _sysTime.Now;
        user.SecurityStamp = GenerateSecurityStamp();

        await UpdateUserAsync(user).ConfigureAwait(false);

        return code;
    }

    private static Guid GenerateSecurityStamp() => Guid.NewGuid();

    public Task UpdateUserAsync(TUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(user);

        _unitOfWork.Users.Update(user);
        return _unitOfWork.CommitChangesAsync(cancellationToken);
    }

    public Task AddUserAsync(TUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(user);

        _unitOfWork.Users.Add(user);
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
