using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Blog.Services.Identity.API.Core;

public class LockoutOptions
{
    [Required]
    public bool EnableAccountLockout { get; set; } = true;

    [Required]
    [Range(0, int.MaxValue)]
    public int MaxAllowedLoginAttempts { get; set; } = 5;

    [Required]
    [TimeSpanValidator(MinValueString = "00:00:00")]
    public TimeSpan AccountLockoutDuration { get; set; } = TimeSpan.FromMinutes(1);
}