namespace Blog.Services.Identity.API.Core;

public class EmailOptions
{
    public bool RequireConfirmed { get; set; } = true;
    public TimeSpan EmailConfirmationCodeValidityPeriod { get; set; } = TimeSpan.FromDays(3);
}
