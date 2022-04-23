using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using NodaTime;

namespace Blog.Services.Blogging.Infrastructure.Services;

public class SysTime : ISysTime
{
    private readonly IClock _clock;

    public SysTime(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Instant Now => _clock.GetCurrentInstant();
}
