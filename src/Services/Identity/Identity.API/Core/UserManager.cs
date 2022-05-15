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
        IdentityResult result = null!;

        if (validatePassword)
            result = await ValidatePasswordAsync(password);

        if (!validatePassword || result == IdentityResult.Success)
        {
            user.PasswordHash = _passwordHasher.HashPassword(password);
            await UpdateUserAsync(user).ConfigureAwait(false);
        }

        return validatePassword ? result : IdentityResult.Success;
    }

    public PasswordVerificationResult VerifyPassword(TUser user, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordVerificationResult.Fail;

        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public Task UpdateLastLoginAndClearAttemptsAsync(TUser user)
    {
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = _sysTime.Now;

        return UpdateUserAsync(user);
    }

    public Task UpdateLastLoginAsync(TUser user)
    {
        user.LastLoginAt = _sysTime.Now;

        return UpdateUserAsync(user);
    }

    public Task AddFailedLoginAttemptAsync(TUser user)
    {
        user.FailedLoginAttempts = ++user.FailedLoginAttempts;
        return UpdateUserAsync(user);
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
        _unitOfWork.Users.Update(user);
        return _unitOfWork.CommitChangesAsync(cancellationToken);
    }

    public ValueTask<ClaimsPrincipal> CreateUserPrincipalAsync(TUser user)
        => _claimsPrincipalFactory.CreateAsync(user);
}
