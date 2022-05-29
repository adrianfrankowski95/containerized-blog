namespace Blog.Services.Identity.API.Core;

public class PasswordOptions
{
    public bool RequireUpperCase { get; set; } = true;
    public bool RequireLowerCase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public int MinLength { get; set; } = 8;
    public int HashWorkFactor { get; set; } = 15;
    public int PasswordResetCodeLength { get; set; } = 6;
    //without I,l
    public string PasswordResetCodeAllowedCharacters { get; set; } = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";
    public TimeSpan PasswordResetCodeValidityPeriod { get; set; } = TimeSpan.FromHours(1);
}