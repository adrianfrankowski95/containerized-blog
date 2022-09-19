using Blog.Services.Identity.Domain.Events;
using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class User : Entity<UserId>, IAggregateRoot
{
    private static readonly Duration _lockoutDuration = Duration.FromMinutes(5);
    public Username Username { get; private set; }
    public FullName FullName { get; private set; }
    public Gender Gender { get; private set; }
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
        Gender gender,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails,
        Instant now,
        PasswordHasher.PasswordHash? passwordHash = null,
        UserRole? userRole = null
    )
    {
        if (username is null)
            throw new ArgumentNullException($"{nameof(Username)} must not be null.");

        if (fullName is null)
            throw new ArgumentNullException($"{nameof(FullName)} must not be null.");

        if (emailAddress is null)
            throw new ArgumentNullException($"{nameof(EmailAddress)} must not be null.");

        if (gender is null)
            throw new IdentityDomainException($"{nameof(Gender)} must not be null.");

        Username = username;
        FullName = fullName;
        Gender = gender;
        EmailAddress = emailAddress;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
        PasswordHash = passwordHash;

        FailedLoginAttemptsCount = FailedLoginAttemptsCount.None;

        Role = userRole ?? UserRole.DefaultRole();
        CreatedAt = now;

        PasswordResetCode = PasswordResetCode.Empty;
        EmailConfirmationCode = EmailConfirmationCode.Empty;
        SecurityStamp = SecurityStamp.NewStamp();
    }

    public static User Register(
        Username username,
        FullName fullName,
        Gender gender,
        EmailAddress emailAddress,
        PasswordHasher.PasswordHash passwordHash,
        bool receiveAdditionalEmails,
        Instant now)
    {
        var user = new User(username, fullName, gender, emailAddress, receiveAdditionalEmails, now, passwordHash);
        user.SetNewEmailConfirmationCode(now);

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Username, user.EmailAddress, user.EmailConfirmationCode));

        return user;
    }

    public static User Create(
        Username username,
        FullName fullName,
        Gender gender,
        EmailAddress emailAddress,
        UserRole role,
        Instant now)
    {
        var user = new User(username, fullName, gender, emailAddress, false, now, null, role);
        user.SetNewPasswordResetCode(now);

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Username, user.EmailAddress, user.PasswordResetCode));

        return user;
    }

    private void ConfirmEmailAddress() => EmailAddress = EmailAddress.Confirm();
    private void SetNewEmailConfirmationCode(Instant now) => EmailConfirmationCode = EmailConfirmationCode.NewCode(now);
    private void UpdateEmailAddress(EmailAddress emailAddress) => EmailAddress = emailAddress;
    private void ClearEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.Empty;
    private void SetNewPasswordResetCode(Instant now) => PasswordResetCode = PasswordResetCode.NewCode(now);
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
    public void SuspendUntil(NonPastInstant until)
    {
        SuspendedUntil = until;
        RefreshSecurityStamp();
    }

    public void Restore() => SuspendedUntil = null;

    public void ConfirmEmailAddress(EmailConfirmationCode providedCode, Instant now)
    {
        EmailConfirmationCode.Verify(providedCode, now);
        ConfirmEmailAddress();
        ClearEmailConfirmationCode();

        AddDomainEvent(new UserConfirmedEmailAddressDomainEvent(Username, EmailAddress));
    }

    public void ResetPassword(Instant now)
    {
        SetNewPasswordResetCode(now);
        ClearPasswordHash();
        RefreshSecurityStamp();

        AddDomainEvent(new UserResetPasswordDomainEvent(Username, EmailAddress, PasswordResetCode));
    }

    public void SetPassword(PasswordHasher.PasswordHash passwordHash, PasswordResetCode providedCode)
    {
        PasswordResetCode.Verify(providedCode);
        UpdatePasswordHash(passwordHash);
        ClearPasswordResetCode();
        RefreshSecurityStamp();

        AddDomainEvent(new UserChangedPasswordDomainEvent(Username, EmailAddress));
    }

    public void SetEmailAddress(EmailAddress emailAddress, Instant now)
    {
        if (emailAddress.IsConfirmed)
            throw new IdentityDomainException("New email address cannot be confirmed.");

        SetNewEmailConfirmationCode(now);
        UpdateEmailAddress(emailAddress);

        AddDomainEvent(new UserChangedEmailAddressDomainEvent(Username, EmailAddress, EmailConfirmationCode));
    }

    public bool UpdatePersonalData(Username? username, FullName? fullName)
    {
        bool isUpdated = false;

        if (username is not null && !Username.Equals(username))
        {
            Username = username;
            isUpdated = true;
        }

        if (fullName is not null && !FullName.Equals(fullName))
        {
            FullName = fullName;
            isUpdated = true;
        }

        return isUpdated;
    }

    public LoginResult LogIn(LoginService loginService, EmailAddress providedEmailAddress, PasswordHasher.PasswordHash providedPasswordHash, Instant now)
    {
        var result = loginService.LogIn(this, providedEmailAddress, providedPasswordHash, now);

        if (result is LoginResult { ErrorCode: LoginErrorCode.InvalidEmail or LoginErrorCode.InvalidPassword })
            FailedLoginAttempt(now);
        else if (result.IsSuccess)
            SuccessfulLoginAttempt(now);

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
    public Guid Value { get; private set; }

    public UserId()
    {
        Value = Guid.NewGuid();
    }

    public static UserId FromGuid(Guid value) => new() { Value = value };

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}