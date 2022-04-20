namespace Blog.Services.Blogging.Domain.SeedWork;

public interface IUnitOfWork : IDisposable
{
    Task CommitChangesAsync(CancellationToken cancellationToken = default);
}
