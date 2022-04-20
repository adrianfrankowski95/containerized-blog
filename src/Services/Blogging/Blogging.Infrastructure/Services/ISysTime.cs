using NodaTime;

namespace Blog.Services.Blogging.Infrastructure.Services;

public interface ISysTime
{
    public IClock Clock { get; }
    public Instant Now();
}
