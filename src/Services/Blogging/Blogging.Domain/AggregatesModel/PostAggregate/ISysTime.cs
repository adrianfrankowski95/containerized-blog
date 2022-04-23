using NodaTime;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public interface ISysTime
{
    public Instant Now { get; }
}
