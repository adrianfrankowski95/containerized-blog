using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Blog.Services.Authorization.API.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    //ef core
    protected ApplicationUser() : base()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public ApplicationUser(string userName) : this()
    {
        UserName = userName;
    }

    public UserRole Role { get; set; } = UserRole.GetDefault();
    public bool ReceiveEmails { get; set; }
    public Language? Language { get; set; }
    public Instant CreatedAt { get; set; }
    public DateTimeOffset? SuspensionEnd { get; set; }
    public Instant? LastLoginAt { get; set; }
    public string? PasswordResetCode { get; set; }
    public Instant? PasswordResetCodeIssuedAt { get; set; }
}