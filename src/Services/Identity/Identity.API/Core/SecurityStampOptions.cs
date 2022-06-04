using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Blog.Services.Identity.API.Core;

public class SecurityStampOptions
{
    [Required]
    [TimeSpanValidator(MinValueString = "00:00:00")]
    public TimeSpan SecurityStampValidationInterval { get; set; } = TimeSpan.FromMinutes(5);
}