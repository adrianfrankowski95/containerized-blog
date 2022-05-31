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
        NonEmptyString email,
        NonEmptyString username,
        NonEmptyString firstName,
        NonEmptyString lastName,
        Gender gender,
        bool receiveAdditionalEmails)
    {
        Id = Guid.NewGuid();

        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        ReceiveAdditionalEmails = receiveAdditionalEmails;
    }

    public Guid Id { get; set; }
    public NonEmptyString Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public NonEmptyString Username { get; set; }
    public NonEmptyString FirstName { get; set; }
    public NonEmptyString LastName { get; set; }
    public Gender Gender { get; set; }
    public Role Role { get; set; }

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