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
    public bool ReceiveAdditionalEmails { get; private set; }
    public UserRole Role { get; private set; }
    public PasswordHasher.PasswordHash? PasswordHash { get; private set; }
    public Instant CreatedAt { get; }
    public FailedLoginAttempts FailedLoginAttempts { get; private set; }
    public Instant? LastLoggedInAt { get; private set; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended(Instant now) => SuspendedUntil is not null && SuspendedUntil < now;
    public bool IsLockedOut(Instant now) => LockedOutUntil is not null && LockedOutUntil < now;
    public bool HasActivePassword => !string.IsNullOrWhiteSpace(PasswordHash!);
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
        if (gender is null)
            throw new IdentityDomainException($"{nameof(Gender)} must not be null.");

        Id = new UserId();

        Username = username;
        FullName = fullName;
        Gender = gender;
        EmailAddress = emailAddress;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
        PasswordHash = passwordHash;

        FailedLoginAttempts = UserAggregate.FailedLoginAttempts.None;

        Role = userRole ?? UserRole.DefaultRole();
        CreatedAt = now;

        PasswordResetCode = PasswordResetCode.Empty;
        EmailConfirmationCode = EmailConfirmationCode.Empty;
        SecurityStamp = SecurityStamp.NewStamp(now);
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
    private void UpdatePasswordHash(PasswordHasher.PasswordHash passwordHash) => PasswordHash = passwordHash;
    private void AddFailedLoginAttempt(Instant now) => FailedLoginAttempts = FailedLoginAttempts.Increment(now);
    private void ClearFailedLoginAttempts() => FailedLoginAttempts = UserAggregate.FailedLoginAttempts.None;
    private void ClearFailedLoginAttemptsIfExpired(Instant now)
    {
        if (!FailedLoginAttempts.IsEmpty && FailedLoginAttempts.IsExpired(now))
            ClearFailedLoginAttempts();
    }
    private void SetSuccessfulLogin(Instant now) => LastLoggedInAt = now;
    private void LockOutUntil(NonPastInstant until)
    {
        LockedOutUntil = until;
        ClearFailedLoginAttempts();
    }
    private void RefreshSecurityStamp(Instant now) => SecurityStamp = SecurityStamp.NewStamp(now);
    public void SuspendUntil(NonPastInstant until, Instant now)
    {
        SuspendedUntil = until;
        RefreshSecurityStamp(now);
    }

    public void Restore() => SuspendedUntil = null;

    public void ConfirmEmailAddress(NonEmptyString providedCode, Instant now)
    {
        EmailConfirmationCode.Verify(providedCode, now);
        ConfirmEmailAddress();
        ClearEmailConfirmationCode();

        AddDomainEvent(new EmailAddressConfirmedDomainEvent(Username, EmailAddress));
    }

    public void RequestPasswordReset(Instant now)
    {
        SetNewPasswordResetCode(now);

        AddDomainEvent(new PasswordResetRequestedDomainEvent(Username, EmailAddress, PasswordResetCode));
    }

    public void ResetPassword(PasswordHasher passwordHasher, Password newPassword, NonEmptyString providedCode, Instant now)
    {
        if (passwordHasher is null)
            throw new IdentityDomainException("Password hash must not be null when resetting a password.");

        PasswordResetCode.Verify(providedCode, now);

        if (passwordHasher.VerifyPasswordHash(newPassword, PasswordHash))
            throw new IdentityDomainException("The new password must be different than the old one.");

        var passwordHash = passwordHasher.HashPassword(newPassword);
        UpdatePasswordHash(passwordHash);
        ClearPasswordResetCode();
        RefreshSecurityStamp(now);

        AddDomainEvent(new PasswordResetDomainEvent(Username, EmailAddress));
    }

    public void SetPassword(PasswordHasher passwordHasher, NonEmptyString newPassword, NonEmptyString oldPassword, Instant now)
    {
        if (passwordHasher is null)
            throw new IdentityDomainException("Password hash must not be null when setting a password.");

        if (!passwordHasher.VerifyPasswordHash(oldPassword, PasswordHash))
            throw new IdentityDomainException("Invalid password.");

        if (passwordHasher.VerifyPasswordHash(newPassword, PasswordHash))
            throw new IdentityDomainException("The new password must be different than the old one.");

        var passwordHash = passwordHasher.HashPassword(newPassword);
        UpdatePasswordHash(passwordHash);
        RefreshSecurityStamp(now);

        AddDomainEvent(new PasswordChangedDomainEvent(Username, EmailAddress));
    }

    public void SetEmailAddress(EmailAddress emailAddress, Instant now)
    {
        if (emailAddress.IsConfirmed)
            throw new IdentityDomainException("New email address cannot be confirmed.");

        SetNewEmailConfirmationCode(now);
        UpdateEmailAddress(emailAddress);

        AddDomainEvent(new EmailAddressChangedDomainEvent(Username, EmailAddress, EmailConfirmationCode));
    }

    public bool UpdatePersonalData(FullName? fullName, bool? receiveAdditionalEmails)
    {
        bool isUpdated = false;

        if (fullName is not null && !FullName.Equals(fullName))
        {
            FullName = fullName.Value;
            isUpdated = true;
        }

        if (receiveAdditionalEmails is not null && !ReceiveAdditionalEmails.Equals(receiveAdditionalEmails))
        {
            ReceiveAdditionalEmails = receiveAdditionalEmails.Value;
            isUpdated = true;
        }

        return isUpdated;
    }

    public LoginResult LogIn(
        LoginService loginService,
        NonEmptyString providedEmailAddress,
        NonEmptyString providedPassword,
        PasswordHasher passwordHasher,
        Instant now)
    {
        var result = loginService.LogIn(this, providedEmailAddress, providedPassword, passwordHasher, now);

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

        if (FailedLoginAttempts.IsMaxAllowed())
            LockOutUntil(new NonPastInstant(now.Plus(_lockoutDuration), now));
        else
            AddFailedLoginAttempt(now);
    }

    private void SuccessfulLoginAttempt(Instant now)
    {
        ClearFailedLoginAttempts();
        SetSuccessfulLogin(now);
    }

    public SecurityStampValidationResult ValidateSecurityStamp(NonEmptyString otherStamp, Instant now)
    {
        var result = SecurityStamp.Validate(otherStamp, now);

        if (result is SecurityStampValidationResult.Valid)
            SecurityStamp = SecurityStamp.ExtendExpiration(now);

        return result;
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