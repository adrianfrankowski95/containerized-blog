using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Blogging.Infrastructure;

public class EfUnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private readonly IdentityDbContext _ctx;
    private bool isDisposed;

    public EfUnitOfWork(IdentityDbContext ctx)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
    }
    public async Task CommitChangesAsync(CancellationToken cancellationToken = default)
    {
        await _ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
            _ctx.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }

    protected async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
            await _ctx.DisposeAsync().ConfigureAwait(false);
    }

}
