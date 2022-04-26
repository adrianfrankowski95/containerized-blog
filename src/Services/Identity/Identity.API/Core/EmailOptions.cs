namespace Blog.Services.Identity.API.Core;

public class EmailOptions
{
    public bool RequireUnique { get; set; } = true;
    public bool RequireConfirmed { get; set; } = true;
}
