namespace Blog.Services.Identity.API.Core;

public class SecurityStampOptions
{
    public TimeSpan SecurityStampValidationInterval { get; set; } = TimeSpan.FromMinutes(5);
}