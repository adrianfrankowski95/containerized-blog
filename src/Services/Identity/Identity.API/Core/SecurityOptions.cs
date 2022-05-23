namespace Blog.Services.Identity.API.Core;

public class SecurityOptions
{
    public bool EnableLoginAttemptsLock { get; set; } = true;
    public int MaxAllowedLoginAttempts { get; set; } = 5;
    public TimeSpan AccountLockDuration { get; set; } = TimeSpan.FromMinutes(1);
    public int PasswordResetCodeLength { get; set; } = 6;
    //without I,l
    public string PasswordResetCodeAllowedCharacters { get; set; } = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    public TimeSpan PasswordResetCodeValidityPeriod { get; set; } = TimeSpan.FromDays(3);
}