using NodaTime;

namespace Blog.Services.Identity.API.Services;

public class SysTime : ISysTime
{
    private readonly IClock _clock;

    public SysTime(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Instant Now => _clock.GetCurrentInstant();
}
