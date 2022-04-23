using NodaTime;

namespace Blog.Services.Identity.API.Services;

public interface ISysTime
{
    public Instant Now { get; }
}
