using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.Events;

public class PostStatusChangedToDeleted : DomainEvent
{
    public PostStatusChangedToDeleted(Guid correlationId, DateTime occuredOn) : base(correlationId, occuredOn)
    {

    }
}
