namespace Blog.Services.Identity.Domain.SeedWork;

public interface IUnitOfWork : IDisposable
{
    Task CommitChangesAsync(CancellationToken cancellationToken = default);
}
