using NodaTime;

namespace Blog.Services.Identity.Infrastructure.Avatar;

public class AvatarModel
{
    public Guid UserId { get; set; }
    public byte[] ImageData { get; set; }
    public string Format { get; set; }
    public Instant UpdatedAt { get; set; }
}
