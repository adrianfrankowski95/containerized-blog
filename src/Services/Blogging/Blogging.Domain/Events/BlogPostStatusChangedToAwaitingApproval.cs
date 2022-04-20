using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.Events;

public class PostStatusChangedToAwaitingApproval : DomainEvent
{
    public PostStatusChangedToAwaitingApproval(Guid correlationId, DateTime occuredOn) : base(correlationId, occuredOn)
    {

    }
}
