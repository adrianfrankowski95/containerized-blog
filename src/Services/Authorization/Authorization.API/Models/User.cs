using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Blog.Services.Authorization.API.Models;

public class User : IdentityUser<Guid>
{
    //ef core
    protected User() : base()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }

    public Role Role { get; set; } = Role.GetDefault();
    public bool ReceiveEmails { get; set; }
    public Language? Language { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? SuspensionEnd { get; set; }
    public Instant? LastLoginAt { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
}