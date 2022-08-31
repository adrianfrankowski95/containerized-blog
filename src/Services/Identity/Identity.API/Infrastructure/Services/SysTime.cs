using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using NodaTime;

namespace Blog.Services.Identity.API.Infrastructure.Services;

public class SysTime : ISysTime
{
    private readonly IClock _clock;
    public Instant Now => _clock.GetCurrentInstant();

    public SysTime(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
}
