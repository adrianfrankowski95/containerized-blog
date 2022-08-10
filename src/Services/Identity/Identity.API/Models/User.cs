using System.Text.Json.Serialization;
using NodaTime;

namespace Blog.Services.Identity.API.Models;

public class User
{
    //ef core
    protected User() : base()
    {

    }

    public User(
        NonEmptyString emailAddress,
        NonEmptyString username,
        NonEmptyString firstName,
        NonEmptyString lastName,
        bool receiveAdditionalEmails)
    {
        Id = Guid.NewGuid();

        EmailAddress = emailAddress;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
    }

    public Guid Id { get; set; }
    public NonEmptyString EmailAddress { get; set; }
    public bool EmailConfirmed { get; set; }
    public NonEmptyString Username { get; set; }
    public NonEmptyString FirstName { get; set; }
    public NonEmptyString LastName { get; set; }
    public UserRole Role { get; set; }

    [JsonIgnore]
    public NonEmptyString FullName => FirstName + " " + LastName;

    public NonEmptyString PasswordHash { get; set; }

    public bool ReceiveAdditionalEmails { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? LockedOutUntil { get; set; }
    public Instant? SuspendedUntil { get; set; }
    public Instant? LastLoginAt { get; set; }
    public NonNegativeInt FailedLoginAttempts { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
    public Guid? EmailConfirmationCode { get; set; }
    public Instant? EmailConfirmationCodeIssuedAt { get; set; }
}