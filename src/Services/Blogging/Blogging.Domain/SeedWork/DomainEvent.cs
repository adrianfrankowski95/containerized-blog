namespace Blog.Services.Blogging.Domain.SeedWork;

public abstract class DomainEvent
{
    public Guid Id { get; }
    public Guid CorrelationId { get; }
    public DateTime OccuredOn { get; }

    protected DomainEvent(Guid correlationId, DateTime occuredOn)
    {
        Id = Guid.NewGuid();
        CorrelationId = correlationId;
        OccuredOn = occuredOn;
    }
}