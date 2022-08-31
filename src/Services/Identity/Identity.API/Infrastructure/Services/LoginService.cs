
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly ISysTime _sysTime;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IUserRepository userRepository, ISysTime sysTime, ILogger<LoginService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResult> LoginAsync(EmailAddress providedEmailAddress, PasswordHasher.PasswordHash providedPasswordHash)
    {
        if (providedEmailAddress is null)
            throw new ArgumentNullException(nameof(providedEmailAddress));

        if (providedPasswordHash is null)
            throw new ArgumentNullException(nameof(providedPasswordHash));

        var user = await _userRepository.FindByEmailAsync(providedEmailAddress).ConfigureAwait(false);

        if (user is null)
            return LoginResult.Fail(LoginErrorCode.UserNotFound);

        var now = _sysTime.Now;

        if (user.IsLockedOut(now))
            return LoginResult.Fail(LoginErrorCode.AccountLockedOut);

        if (user.IsSuspended(now))
        {
            user.FailedLoginAttempt(now);
            return LoginResult.Fail(LoginErrorCode.AccountSuspended);
        }

        if (!user.HasActivePassword)
        {
            user.FailedLoginAttempt(now);
            return LoginResult.Fail(LoginErrorCode.InactivePassword);
        }

        if (!user.PasswordHash!.Equals(providedPasswordHash))
        {
            user.FailedLoginAttempt(now);
            return LoginResult.Fail(LoginErrorCode.InvalidPassword);
        }

        if (!user.HasConfirmedEmailAddress)
        {
            user.FailedLoginAttempt(now);
            return LoginResult.Fail(LoginErrorCode.UnconfirmedEmail);
        }

        user.SuccessfulLoginAttempt(now);
        return LoginResult.Success;
    }
}
