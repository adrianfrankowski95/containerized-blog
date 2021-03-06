using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUnitOfWork<TUser> : IDisposable where TUser : User
{
    public IUserRepository<TUser> Users { get; }
    Task CommitChangesAsync(CancellationToken cancellationToken = default);
}
