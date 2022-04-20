using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.Events;

public class PostStatusChangedToDraft : DomainEvent
{
    public PostStatusChangedToDraft(Guid correlationId, DateTime occuredOn) : base(correlationId, occuredOn)
    {

    }
}
