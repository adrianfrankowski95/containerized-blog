namespace Blog.Services.Identity.Domain.SeedWork;
public abstract class Entity<T>
{
    public T Id { get; protected set; } = default!;
    private List<DomainEvent> _domainEvents;
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents ??= new List<DomainEvent>();
        _domainEvents.Add(@event);
    }

    public void RemoveDomainEvent(DomainEvent @event)
    {
        _domainEvents?.Remove(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }
}