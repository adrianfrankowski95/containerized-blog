using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;
using Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class User : Entity<UserId>, IAggregateRoot
{
    public PasswordHash PasswordHash { get; private set; }
    public NonEmptyString Username { get; }
    public NonEmptyString FirstName { get; }
    public NonEmptyString LastName { get; }
    public NonEmptyString FullName => FirstName + " " + LastName;
    public EmailAddress EmailAddress { get; private set; }
    public bool ReceiveAdditionalEmails { get; }
    public UserRole Role { get; private set; }
    public Instant CreatedAt { get; }
    public Instant? LockedOutUntil { get; private set; }
    public Instant? SuspendedUntil { get; private set; }
    public Instant? LastLoginAt { get; private set; }
    public NonNegativeInt FailedLoginAttempts { get; private set; }
    public PasswordResetCode PasswordResetCode { get; private set; }
    public EmailConfirmationCode EmailConfirmationCode { get; private set; }
    public SecurityStamp SecurityStamp { get; private set; }

    public bool IsSuspended => SuspendedUntil is not null || SuspendedUntil < SystemClock.Instance.GetCurrentInstant();
    public bool IsLockedOut => LockedOutUntil is not null || LockedOutUntil < SystemClock.Instance.GetCurrentInstant();

    public User(NonEmptyString username, NonEmptyString firstName, NonEmptyString lastName, EmailAddress emailAddress, bool receiveAdditionalEmails)
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
        FailedLoginAttempts = 0;

        Role = UserRole.DefaultRole();
        CreatedAt = SystemClock.Instance.GetCurrentInstant();

        PasswordResetCode = PasswordResetCode.EmptyCode();
        EmailConfirmationCode = EmailConfirmationCode.EmptyCode();
        SecurityStamp = SecurityStamp.NewStamp();
    }

    public void ConfirmEmailAddress(EmailConfirmationCode providedCode)
    {
        if (!EmailConfirmationCode.Verify(providedCode))
            throw new IdentityDomainException("Error verifying email confirmation code.");

        EmailAddress = EmailAddress.Confirm();
        ResetEmailConfirmationCode();
    }

    public void SetNewPassword(Password password, IPasswordHasher passwordHasher, PasswordResetCode providedCode)
    {
        if (!PasswordResetCode.Verify(providedCode))
            throw new IdentityDomainException("Error verifying password reset code.");

        PasswordHash = new(password, passwordHasher);

        ResetPasswordResetCode();
        RefreshSecurityStamp();
    }

    private void ResetPasswordResetCode() => PasswordResetCode = PasswordResetCode.EmptyCode();
    private void ResetEmailConfirmationCode() => EmailConfirmationCode = EmailConfirmationCode.EmptyCode();
    private void RefreshSecurityStamp() => SecurityStamp = SecurityStamp.NewStamp();

}

public class UserId
{
    public Guid Value { get; }

    public UserId()
    {
        Value = Guid.NewGuid();
    }
}