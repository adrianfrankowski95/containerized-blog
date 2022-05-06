namespace Blog.Services.Identity.API.Core;

public class SecurityOptions
{
    public bool EnableLoginAttemptsLock { get; set; } = true;
    public int MaxAllowedLoginAttempts { get; set; } = 10;
    public int AccountLockDurationMinutes { get; set; } = 15;
    public int PasswordResetCodeLength { get; set; } = 6;
    //without I,l
    public string PasswordResetCodeAllowedCharacters { get; set; } = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    public int PasswordResetCodeExpirationDays { get; set; } = 3;
}