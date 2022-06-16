using NodaTime;

namespace Blog.Services.Identity.API.Core;

public interface ISysTime
{
    public Instant Now { get; }
}
