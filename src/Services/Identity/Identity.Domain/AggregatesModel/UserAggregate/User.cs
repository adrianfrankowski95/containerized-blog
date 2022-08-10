using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class User : Entity<UserId>, IAggregateRoot
{
    public NonEmptyString Username { get; }
    public NonEmptyString FirstName { get; }
    public NonEmptyString LastName { get; }
    public NonEmptyString FullName => FirstName + " " + LastName;
    public EmailAddress EmailAddress { get; private set; }
    public bool ReceiveAdditionalEmails { get; }
    public UserRole Role { get; private set; }
    public PasswordHash? PasswordHash { get; private set; }
    public Instant CreatedAt { get; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public Instant? LastLoginAt { get; private set; }
    public NonNegativeInt FailedLoginAttempts { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended => SuspendedUntil is not null && SuspendedUntil < SystemClock.Instance.GetCurrentInstant();
    public bool IsLockedOut => LockedOutUntil is not null && LockedOutUntil < SystemClock.Instance.GetCurrentInstant();
    public bool IsPasswordActive => PasswordResetCode.IsEmpty() && PasswordHash is not null;
    public bool IsEmailAddressConfirmed => EmailAddress.IsConfirmed;
    public bool CanLogin => !IsLockedOut && !IsSuspended && IsPasswordActive && IsEmailAddressConfirmed;

    private User(
        NonEmptyString username,
        NonEmptyString firstName,
        NonEmptyString lastName,
        EmailAddress emailAddress
    )
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;

        FailedLoginAttempts = 0;  

        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        Role = UserRole.DefaultRole();

        PasswordResetCode = PasswordResetCode.EmptyCode();
        EmailConfirmationCode = EmailConfirmationCode.NewCode();
        SecurityStamp = SecurityStamp.NewStamp();
    }

    private User(
        NonEmptyString username,
        NonEmptyString firstName,
        NonEmptyString lastName,
        EmailAddress emailAddress,
        UserRole userRole) : this(username, firstName, lastName, emailAddress)
    {
        Role = userRole;
    }

    private User(
        NonEmptyString username,
        PasswordHash passwordHash,
        NonEmptyString firstName,
        NonEmptyString lastName,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails) : this(username, firstName, lastName, emailAddress)
    {
        PasswordHash = passwordHash;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
    }

    public static User Create(
        NonEmptyString username,
        PasswordHash passwordHash,
        NonEmptyString firstName,
        NonEmptyString lastName,
        EmailAddress emailAddress,
        bool receiveAdditionalEmails)
        // TODO:
        // - issue user created event that will be consumed by emailing service
            => new User(username, passwordHash, firstName, lastName, emailAddress, receiveAdditionalEmails);

    public static User CreateBy(
        User currentUser,
        NonEmptyString username,
        NonEmptyString firstName,
        NonEmptyString lastName,
        EmailAddress emailAddress,
        UserRole role)
    {
        // TODO:
        // - check if current user has permission to create users
        // and if provided role is allowed for current user's role
        // - issue user created event that will be consumed by emailing service
        return new User(username, firstName, lastName, emailAddress, role);
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
        if(IsSuspended)
            throw new IdentityDomainException($"Account is suspended until {SuspendedUntil.ToString()}.");
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
}

public class UserId
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
}