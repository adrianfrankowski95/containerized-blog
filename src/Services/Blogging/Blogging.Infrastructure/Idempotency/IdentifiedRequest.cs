using NodaTime;

namespace Blog.Services.Blogging.Infrastructure.Idempotency;

public class IdentifiedRequest
{
    public Guid Id { get; }
    public string Type { get; }
    public Instant CreatedAt { get; }

    public IdentifiedRequest(Guid id, string type, Instant createdAt)
    {
        Id = id;
        Type = type;
        CreatedAt = createdAt;
    }
}
