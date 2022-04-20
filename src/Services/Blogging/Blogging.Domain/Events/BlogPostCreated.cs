using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.Events;

public class PostCreated : DomainEvent
{
    public PostCreated(Guid correlationId, DateTime occuredOn) : base(correlationId, occuredOn)
    {

    }
}
