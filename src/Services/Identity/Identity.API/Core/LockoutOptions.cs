namespace Blog.Services.Identity.API.Core;

public class LockoutOptions
{
    public bool EnableAccountLockout { get; set; } = true;
    public int MaxAllowedLoginAttempts { get; set; } = 5;
    public TimeSpan AccountLockoutDuration { get; set; } = TimeSpan.FromMinutes(1);
}