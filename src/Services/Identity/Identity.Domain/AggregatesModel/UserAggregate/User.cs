using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class User : Entity<UserId>, IAggregateRoot
{
    private static readonly Duration _lockoutDuration = Duration.FromMinutes(5);
    public Username Username { get; }
    public FullName FullName { get; }
    public EmailAddress EmailAddress { get; private set; }
    public bool ReceiveAdditionalEmails { get; }
    public UserRole Role { get; private set; }
    public PasswordHasher.PasswordHash? PasswordHash { get; private set; }
    public Instant CreatedAt { get; }
    public FailedLoginAttemptsCount FailedLoginAttemptsCount { get; private set; }
    public Instant? LastLoggedInAt { get; private set; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended(Instant now) => SuspendedUntil is not null && SuspendedUntil < now;
    public bool IsLockedOut(Instant now) => LockedOutUntil is not null && LockedOutUntil < now;
    public bool HasActivePassword => PasswordHash is not null && PasswordResetCode.IsEmpty();
    public bool HasConfirmedEmailAddress => EmailAddress.IsConfirmed;

    private User(
        Username username,
        FullName fullName,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails,
        Instant now,
        PasswordHasher.PasswordHash? passwordHash = null,
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
        CreatedAt = now;

        PasswordResetCode = passwordHash is null ? PasswordResetCode.Empty : passwordResetCode!;
        EmailConfirmationCode = EmailConfirmationCode.NewCode(now);
        SecurityStamp = SecurityStamp.NewStamp();
    }

    public static User Create(
        Username username,
        FullName fullName,
        PasswordHasher.PasswordHash passwordHash,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails,
        Instant now)
            // TODO:
            // - issue user created event that will be consumed by emailing service
            => new(username, fullName, emailAddress, receiveAdditionalEmails, now, passwordHash);

    public static User CreateBy(
        User currentUser,
        Username username,
        FullName fullName,
        EmailAddress emailAddress,
        UserRole role,
        Instant now)
    {
        // TODO:
        // - check if current user has permission to create users
        // and if provided role is allowed for current user's role
        // - issue user created event that will be consumed by emailing service
        var code = PasswordResetCode.NewCode();
        return new User(username, fullName, emailAddress, false, now, null, code, role);
    }

    private void ConfirmEmailAddress() => EmailAddress = EmailAddress.Confirm();
    private void SetNewEmailConfirmationCode(Instant now) => EmailConfirmationCode = EmailConfirmationCode.NewCode(now);
    private void ClearEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.Empty;
    private void SetNewPasswordResetCode() => PasswordResetCode = PasswordResetCode.NewCode();
    private void ClearPasswordResetCode() => PasswordResetCode = PasswordResetCode.Empty;
    private void ClearPasswordHash() => PasswordHash = null;
    private void UpdatePasswordHash(PasswordHasher.PasswordHash passwordHash) => PasswordHash = passwordHash;
    private void AddFailedLoginAttempt(Instant now) => FailedLoginAttemptsCount = FailedLoginAttemptsCount.Increment(now);
    private void ClearFailedLoginAttempts() => FailedLoginAttemptsCount = FailedLoginAttemptsCount.None;
    private void ClearFailedLoginAttemptsIfExpired(Instant now)
    {
        if (!FailedLoginAttemptsCount.IsEmpty() && FailedLoginAttemptsCount.IsExpired(now))
            ClearFailedLoginAttempts();
    }
    private void SetSuccessfulLogin(Instant now) => LastLoggedInAt = now;
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
    public void ConfirmEmailAddress(EmailConfirmationCode providedCode, Instant now)
    {
        EmailConfirmationCode.Verify(providedCode, now);
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

    public void SetPassword(PasswordHasher.PasswordHash passwordHash, PasswordResetCode providedCode)
    {
        // TODO:
        // - issue password updated event that will be consumed by emailing service
        PasswordResetCode.Verify(providedCode);
        UpdatePasswordHash(passwordHash);
        ClearPasswordResetCode();
        RefreshSecurityStamp();
    }

    public LoginResult Login(EmailAddress providedEmailAddress, PasswordHasher.PasswordHash providedPasswordHash, ILoginService loginService, Instant now)
    {
        var result = loginService.Login(this, providedEmailAddress, providedPasswordHash, now);

        if (result.IsSuccess)
            SuccessfulLoginAttempt(now);
        else
            FailedLoginAttempt(now);

        return result;
    }

    private void FailedLoginAttempt(Instant now)
    {
        if (IsLockedOut(now))
            throw new IdentityDomainException("Account has already been locked out.");

        ClearFailedLoginAttemptsIfExpired(now);

        if (FailedLoginAttemptsCount.IsMaxAllowed())
            LockOutUntil(new NonPastInstant(now.Plus(_lockoutDuration), now));
        else
            AddFailedLoginAttempt(now);
    }

    private void SuccessfulLoginAttempt(Instant now)
    {
        ClearFailedLoginAttempts();
        SetSuccessfulLogin(now);
    }
}

public class UserId : ValueObject<UserId>
{
    public Guid Value { get; }

    public UserId()
    {
        Value = Guid.NewGuid();
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}