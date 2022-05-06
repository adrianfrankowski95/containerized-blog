using NodaTime;

namespace Blog.Services.Identity.API.Models;

public abstract class UserBase
{
    //ef core
    protected UserBase()
    {

    }

    public UserBase(NonEmptyString username, NonEmptyString email, NonEmptyString passwordHash)
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid();

        Username = username;
        Email = email;
        PasswordHash = passwordHash;

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
    public bool ReceiveEmails { get; set; }
    public NonEmptyString PasswordHash { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? LockedUntil { get; set; }
    public Instant? SuspendedUntil { get; set; }
    public Instant? LastLoginAt { get; set; }
    public NonNegativeNumber FailedLoginAttempts { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
}