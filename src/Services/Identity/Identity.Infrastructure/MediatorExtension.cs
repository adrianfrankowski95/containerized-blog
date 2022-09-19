using Blog.Services.Identity.Domain.SeedWork;
using MediatR;

namespace Blog.Services.Identity.Infrastructure;

public static class MediatorExtension
{
    public static Task DispatchDomainEventsAsync(this IMediator mediator, IdentityDbContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity<object>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents is not null && e.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .Select(e => e.DomainEvents)
            .ToList();

        var publishTasks = new List<Task>(domainEvents.Count);
        domainEvents.ForEach(@event => publishTasks.Add(mediator.Publish(@event)));

        domainEntities.ForEach(e => e.ClearDomainEvents());

        return Task.WhenAll(publishTasks);
    }
}
