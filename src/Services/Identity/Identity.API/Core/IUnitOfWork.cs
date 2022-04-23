namespace Blog.Services.Identity.API.Core;

public interface IUnitOfWork : IDisposable
{
    Task CommitChangesAsync(CancellationToken cancellationToken = default);
}
