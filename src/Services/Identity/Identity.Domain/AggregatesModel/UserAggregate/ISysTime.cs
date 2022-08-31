using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface ISysTime
{
    public Instant Now { get; }
}
