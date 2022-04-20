using NodaTime;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public static class SysTime
{
    private static IClock Clock => SystemClock.Instance;
    public static Instant Now() => Clock.GetCurrentInstant();
}
