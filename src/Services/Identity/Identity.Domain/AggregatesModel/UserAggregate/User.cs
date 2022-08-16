using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class User : Entity<UserId>, IAggregateRoot
{
    public Username Username { get; }
    public FullName FullName { get; }
    public EmailAddress EmailAddress { get; private set; }
    public bool ReceiveAdditionalEmails { get; }
    public UserRole Role { get; private set; }
    public PasswordHash? PasswordHash { get; private set; }
    public Instant CreatedAt { get; }
    public FailedLoginAttemptsCount FailedLoginAttemptsCount { get; private set; }
    public Instant? LastLoggedInAt { get; private set; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended => SuspendedUntil is not null && SuspendedUntil < SystemClock.Instance.GetCurrentInstant();
    public bool IsLockedOut => LockedOutUntil is not null && LockedOutUntil < SystemClock.Instance.GetCurrentInstant();
    public bool HasActivePassword => PasswordHash is not null && PasswordResetCode.IsEmpty();
    public bool HasConfirmedEmailAddress => EmailAddress.IsConfirmed;

    private User(
        Username username,
        FullName fullName,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails,
        PasswordHash? passwordHash = null,
        PasswordResetCode? passwordResetCode = null,
        UserRole? userRole = null
    )
    {
        if (username is null)
            throw new ArgumentNullException($"{nameof(Username)} must not be null.");

        if (fullName is null)
            throw new ArgumentNullException($"{nameof(FullName)} must not be null.");

        if (emailAddress is null)
            throw new ArgumentNullException($"{nameof(EmailAddress)} must not be null.");

        if (passwordHash is null && passwordResetCode is not null)
            throw new IdentityDomainException("Password reset code must not be null while creating user without password.");

        Username = username;
        FullName = fullName;
        EmailAddress = emailAddress;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
        PasswordHash = passwordHash;

        FailedLoginAttemptsCount = FailedLoginAttemptsCount.None;

        Role = userRole ?? UserRole.DefaultRole();
        CreatedAt = SystemClock.Instance.GetCurrentInstant();

        PasswordResetCode = passwordHash is null ? PasswordResetCode.Empty : passwordResetCode!;
        EmailConfirmationCode = EmailConfirmationCode.NewCode();
        SecurityStamp = SecurityStamp.NewStamp();
    }

    public static User Create(
        Username username,
        FullName fullName,
        PasswordHash passwordHash,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails)
            // TODO:
            // - issue user created event that will be consumed by emailing service
            => new(username, fullName, emailAddress, receiveAdditionalEmails, passwordHash);

    public static User CreateBy(
        User currentUser,
        Username username,
        FullName fullName,
        EmailAddress emailAddress,
        UserRole role)
    {
        // TODO:
        // - check if current user has permission to create users
        // and if provided role is allowed for current user's role
        // - issue user created event that will be consumed by emailing service
        var code = PasswordResetCode.NewCode();
        return new User(username, fullName, emailAddress, false, null, code, role);
    }

    private void ConfirmEmailAddress() => EmailAddress = EmailAddress.Confirm();
    private void SetNewEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.NewCode();
    private void ClearEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.Empty;
    private void SetNewPasswordResetCode() => PasswordResetCode = PasswordResetCode.NewCode();
    private void ClearPasswordResetCode() => PasswordResetCode = PasswordResetCode.Empty;
    private void ClearPasswordHash() => PasswordHash = null;
    private void UpdatePasswordHash(PasswordHash passwordHash) => PasswordHash = passwordHash;
    private void AddFailedLoginAttempt() => FailedLoginAttemptsCount = FailedLoginAttemptsCount.Increment();
    private void ClearFailedLoginAttempts() => FailedLoginAttemptsCount = FailedLoginAttemptsCount.None;
    private void ClearFailedLoginAttemptsIfExpired()
    {
        if (!FailedLoginAttemptsCount.IsEmpty() && FailedLoginAttemptsCount.IsExpired())
            ClearFailedLoginAttempts();
    }
    private void SetSuccessfulLogin() => LastLoggedInAt = SystemClock.Instance.GetCurrentInstant();
    private void LockOutUntil(NonPastInstant until)
    {
        LockedOutUntil = until;
        ClearFailedLoginAttempts();
    }
    private void RefreshSecurityStamp() => SecurityStamp = SecurityStamp.NewStamp();
    public void SuspendUntilBy(User currentUser, NonPastInstant until)
    {
        // TODO:
        // - check if current user has permission to suspend users
        SuspendedUntil = until;
    }
    public void ConfirmEmailAddress(EmailConfirmationCode providedCode)
    {
        EmailConfirmationCode.Verify(providedCode);
        ConfirmEmailAddress();
        ClearEmailConfirmationCode();
    }

    public void ResetPassword()
    {
        // TODO:
        // - issue password reset event that will be consumed by emailing service
        SetNewPasswordResetCode();
        ClearPasswordHash();
        RefreshSecurityStamp();
    }

    public void UpdatePassword(PasswordHash passwordHash, PasswordResetCode providedCode)
    {
        // TODO:
        // - issue password updated event that will be consumed by emailing service
        PasswordResetCode.Verify(providedCode);
        UpdatePasswordHash(passwordHash);
        ClearPasswordResetCode();
        RefreshSecurityStamp();
    }

    public LoginResult Login(EmailAddress emailAddress, PasswordHash passwordHash)
    {
        if (IsLockedOut)
            return LoginResult.Fail(LoginErrorCode.AccountLockedOut);

        if (emailAddress is null || passwordHash is null || !HasActivePassword ||
            !EmailAddress.Equals(emailAddress) || !PasswordHash!.Equals(passwordHash))
        {
            FailedLoginAttempt();
            return LoginResult.Fail(LoginErrorCode.InvalidCredentials);
        }

        if (IsSuspended)
        {
            FailedLoginAttempt();
            return LoginResult.Fail(LoginErrorCode.AccountSuspended);
        }

        if (!HasConfirmedEmailAddress)
        {
            FailedLoginAttempt();
            return LoginResult.Fail(LoginErrorCode.UnconfirmedEmail);
        }

        SuccessfulLoginAttempt();
        return LoginResult.Success;
    }

    private void FailedLoginAttempt()
    {
        if (IsLockedOut)
            throw new IdentityDomainException("Account has already been locked out.");

        ClearFailedLoginAttemptsIfExpired();

        if (FailedLoginAttemptsCount.IsMaxAllowed())
            LockOutUntil(SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(5)));
        else
            AddFailedLoginAttempt();
    }

    private void SuccessfulLoginAttempt()
    {
        ClearFailedLoginAttempts();
        SetSuccessfulLogin();
    }
}

public class UserId : ValueObject<UserId>
{
    public Guid Value { get; }

    public UserId()
    {
        Value = Guid.NewGuid();
    }

    private UserId(Guid guid)
    {
        Value = guid;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}