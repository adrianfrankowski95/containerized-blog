using NodaTime;

namespace Blog.Services.Users.API.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool EmailVerified { get; set; }
    public string Password { get; set; }
    public UserRole UserRole { get; set; }
    public bool ReceiveEmails { get; set; }
    public Language Language { get; set; }
    public Instant RegisteredAt { get; set; }
    public Instant? BlockedUntil { get; set; }
    public Instant? LastLogin { get; set; }
    public Instant? UpdatedAt { get; set; }
    public Guid SecurityStamp { get; set; }
    public string? PasswordRecoveryCode { get; set; }
}