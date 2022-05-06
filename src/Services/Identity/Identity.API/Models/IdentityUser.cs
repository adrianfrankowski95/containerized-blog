using NodaTime;

namespace Blog.Services.Identity.API.Models;

public class IdentityUser : UserBase
{
    //ef core
    protected IdentityUser() : base()
    {

    }

    public IdentityUser(
        NonEmptyString username,
        NonEmptyString email,
        NonEmptyString passwordHash,
        UserRole userRole,
        bool receiveEmails,
        Language language)
    {

        Role = userRole;
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
    public Language? Language { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? LockedUntil { get; set; }
    public Instant? SuspendedUntil { get; set; }
    public Instant? LastLoginAt { get; set; }
    public NonNegativeNumber FailedLoginAttempts { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
}