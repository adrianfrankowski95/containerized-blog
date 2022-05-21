using NodaTime;

namespace Blog.Services.Identity.API.Models;

public class User
{
    //ef core
    protected User() : base()
    {

    }

    public User(
        NonEmptyString username,
        NonEmptyString email,
        NonEmptyString passwordHash,
        bool receiveEmails)
    {
        Id = Guid.NewGuid();

        Username = username;
        Email = email;
        ReceiveEmails = receiveEmails;
        PasswordHash = passwordHash;

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
    public Role Role { get; set; }
    public bool ReceiveEmails { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? LockedUntil { get; set; }
    public Instant? SuspendedUntil { get; set; }
    public Instant? LastLoginAt { get; set; }
    public NonNegativeInt FailedLoginAttempts { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
}