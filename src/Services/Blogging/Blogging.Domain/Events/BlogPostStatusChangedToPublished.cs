using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.Events;

public class PostStatusChangedToPublished : DomainEvent
{
    public PostStatusChangedToPublished(Guid correlationId, DateTime occuredOn) : base(correlationId, occuredOn)
    {

    }
}
