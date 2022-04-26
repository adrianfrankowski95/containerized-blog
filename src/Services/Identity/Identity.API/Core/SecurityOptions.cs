namespace Blog.Services.Identity.API.Core;

public class SecurityOptions
{
    public int MaxAllowedLoginAttempts { get; set; } = 10;
    public int AccountLockDurationMinutes { get; set; } = 15;
    public int PasswordResetCodeLength { get; set; } = 6;
}