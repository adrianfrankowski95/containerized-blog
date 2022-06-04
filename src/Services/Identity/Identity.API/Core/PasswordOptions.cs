using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Blog.Services.Identity.API.Core;

public class PasswordOptions
{
    [Required]
    public bool RequireUpperCase { get; set; } = true;

    [Required]
    public bool RequireLowerCase { get; set; } = true;

    [Required]
    public bool RequireDigit { get; set; } = true;

    [Required]
    public bool RequireNonAlphanumeric { get; set; } = true;

    [Required]
    [Range(0, int.MaxValue)]
    public int MinLength { get; set; } = 8;

    [Required]
    [Range(0, int.MaxValue)]
    public int HashWorkFactor { get; set; } = 15;

    [Required]
    [Range(0, int.MaxValue)]
    public int PasswordResetCodeLength { get; set; } = 6;

    
    [Required]
    [StringLength(int.MaxValue, MinimumLength = 1)]
    //without I,l
    public string PasswordResetCodeAllowedCharacters { get; set; } = "ABCDEFGHJKLMNOPQRSTUVWXYZ1234567890!@$?_-/\\=abcdefghijkmnopqrstuvwxyz";

    [Required]
    [TimeSpanValidator(MinValueString = "00:00:00")]
    public TimeSpan PasswordResetCodeValidityPeriod { get; set; } = TimeSpan.FromHours(1);
}