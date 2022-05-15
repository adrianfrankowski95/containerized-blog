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

    public bool ReceiveEmails { get; set; }
    public Language? Language { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? SuspensionEnd { get; set; }
    public Instant? LastLoginAt { get; set; }
}