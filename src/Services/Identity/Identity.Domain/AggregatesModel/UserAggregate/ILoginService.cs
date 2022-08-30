using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface ILoginService
{
    public LoginResult Login(User user, EmailAddress providedEmailAddress, PasswordHasher.PasswordHash providedPasswordHash, Instant now);

    //     if (IsLockedOut(now))
    //             return LoginResult.Fail(LoginErrorCode.AccountLockedOut);

    //         if (emailAddress is null || passwordHash is null || !HasActivePassword ||
    //             !EmailAddress.Equals(emailAddress) || !PasswordHash!.Equals(passwordHash))
    //         {
    //             FailedLoginAttempt();
    //             return LoginResult.Fail(LoginErrorCode.InvalidCredentials);
    //         }

    //         if (IsSuspended(now))
    //         {
    //             FailedLoginAttempt();
    //             return LoginResult.Fail(LoginErrorCode.AccountSuspended);
    //         }

    //         if (!HasConfirmedEmailAddress)
    // {
    //     FailedLoginAttempt();
    //     return LoginResult.Fail(LoginErrorCode.UnconfirmedEmail);
    // }

    // SuccessfulLoginAttempt();
    // return LoginResult.Success;
}