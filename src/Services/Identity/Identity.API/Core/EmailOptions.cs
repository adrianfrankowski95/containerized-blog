using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Blog.Services.Identity.API.Core;

public class EmailOptions
{
    [Required]
    public bool RequireConfirmed { get; set; } = true;

    [Required]
    [TimeSpanValidator(MinValueString = "00:00:00")]
    public TimeSpan EmailConfirmationCodeValidityPeriod { get; set; } = TimeSpan.FromDays(3);
}
