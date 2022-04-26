using Blog.Services.Identity.API.Models;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class User
{
    //ef core
    protected User()
    {

    }

    public User(
        NonEmptyString username,
        NonEmptyString email,
        NonEmptyString passwordHash,
        UserRole userRole,
        bool receiveEmails,
        Language language)
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid();

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = userRole;
        ReceiveEmails = receiveEmails;
        Language = language;

        CreatedAt = SystemClock.Instance.GetCurrentInstant();

        FailedLoginAttempts = 0;
        EmailConfirmed = false;
        LockedUntil = null;
        LastLoginAt = null;
        PasswordResetCode = null;
    }

    public Guid Id { get; set; }
    public NonEmptyString Username { get; set; }
    public NonEmptyString Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public NonEmptyString PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool ReceiveEmails { get; set; }
    public Language Language { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? LockedUntil { get; set; }
    public Instant? SuspendedUntil { get; set; }
    public Instant? LastLoginAt { get; set; }
    public NonNegativeNumber FailedLoginAttempts { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordResetCode { get; set; }
    public bool IsResettingPassword => !string.IsNullOrWhiteSpace(PasswordResetCode);
    public bool LockExists => LockedUntil is not null;
    public bool IsCurrentlyLocked => LockExists && LockedUntil > SystemClock.Instance.GetCurrentInstant();
    public bool SuspensionExists => SuspendedUntil is not null;
    public bool IsCurrentlySuspended => SuspensionExists && SuspendedUntil > SystemClock.Instance.GetCurrentInstant();
}