using System.Text.Json.Serialization;
using NodaTime;

namespace Blog.Services.Identity.API.Models;

public class User
{
    //ef core
    protected User() : base()
    {

    }

    public User(NonEmptyString username, NonEmptyString name, NonEmptyString lastName, NonEmptyString email, bool receiveEmails)
    {
        Id = Guid.NewGuid();

        Username = username;
        Name = name;
        LastName = lastName;
        Email = email;
        ReceiveAdditionalEmails = receiveEmails;
    }

    public Guid Id { get; set; }
    public NonEmptyString Username { get; set; }
    public NonEmptyString Name { get; set; }
    public NonEmptyString LastName { get; set; }

    [JsonIgnore]
    public NonEmptyString FullName => Name + " " + LastName;

    public NonEmptyString Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public NonEmptyString PasswordHash { get; set; }
    public Role Role { get; set; }
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