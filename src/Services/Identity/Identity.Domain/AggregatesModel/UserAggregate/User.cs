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
    public Login Login { get; private set; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended => SuspendedUntil is not null && SuspendedUntil < SystemClock.Instance.GetCurrentInstant();
    public bool IsLockedOut => LockedOutUntil is not null && LockedOutUntil < SystemClock.Instance.GetCurrentInstant();
    public bool HasActivePassword => PasswordHash is not null && PasswordResetCode.IsEmpty();
    public bool HasConfirmedEmailAddress => EmailAddress.IsConfirmed;
    public bool CanLogin => !IsLockedOut && !IsSuspended && HasActivePassword && HasConfirmedEmailAddress;

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
            throw new IdentityDomainException("Username is required.");

        if (fullName is null)
            throw new IdentityDomainException("Full name is required.");

        if (emailAddress is null)
            throw new IdentityDomainException("Email address is required.");

        if (passwordHash is null && passwordResetCode is not null)
            throw new IdentityDomainException("Missing password reset code while creating user without password.");

        Username = username;
        FullName = fullName;
        EmailAddress = emailAddress;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
        PasswordHash = passwordHash;

        Login = new();

        Role = userRole ?? UserRole.DefaultRole();
        CreatedAt = SystemClock.Instance.GetCurrentInstant();

        PasswordResetCode = passwordHash is null ? PasswordResetCode.EmptyCode() : passwordResetCode!;
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
    private void ClearEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.EmptyCode();
    private void SetNewPasswordResetCode() => PasswordResetCode = PasswordResetCode.NewCode();
    private void ClearPasswordResetCode() => PasswordResetCode = PasswordResetCode.EmptyCode();
    private void ClearPasswordHash() => PasswordHash = null;
    private void UpdatePasswordHash(PasswordHash passwordHash) => PasswordHash = passwordHash;
    private void RefreshSecurityStamp() => SecurityStamp = SecurityStamp.NewStamp();
    private void DisallowIfSuspended()
    {

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

    public void LoginSuccessfully()
    {
        if (IsLockedOut)
            throw new IdentityDomainException("Account has temporarily been locked out due to exceeded login attempts.");

        if (IsSuspended)
            throw new IdentityDomainException($"Account is suspended until {SuspendedUntil}.");

        if (!HasConfirmedEmailAddress)
            throw new IdentityDomainException("Account has not been confirmed.");

        if (!HasActivePassword)
            throw new IdentityDomainException("Invalid Email Address or Password.");

        Login = Login.SuccessfulAttempt();
    }

    public void FailedLoginAttempt()
        => Login = Login.FailedAttempt(this);

    public void LockOutUntil(NonPastInstant until) => LockedOutUntil = until;
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

    public static UserId FromGuid(Guid guid) => new(guid);

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}