using NodaTime;

namespace Blog.Services.Blogging.Infrastructure.Services;

public class SysTime : ISysTime
{
    public IClock Clock => SystemClock.Instance;

    public Instant Now() => Clock.GetCurrentInstant();
}
