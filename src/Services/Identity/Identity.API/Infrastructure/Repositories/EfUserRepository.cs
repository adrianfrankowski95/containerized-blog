using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Infrastructure.Repositories;

public class EfUserRepository<TUser> : IUserRepository<TUser> where TUser : User
{
    private readonly DbSet<TUser> _users;
    public IUnitOfWork UnitOfWork { get; }

    public EfUserRepository(IdentityDbContext<TUser> ctx, IUnitOfWork unitOfWork)
    {
        _users = ctx.Users ?? throw new ArgumentNullException(nameof(ctx));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public void Add(TUser user)
    {
        _users.Add(user);
    }

    public void Delete(TUser user)
    {
        _users.Remove(user);
    }

    public void Update(TUser user)
    {
        _users.Update(user);
    }

    public async Task<TUser?> FindByIdAsync(Guid userId)
    {
        return await _users
            .FindAsync(userId)
            .ConfigureAwait(false);
    }

    public async Task<IList<TUser>> FindByEmailAsync(string email)
    {
        return await _users
            .Where(x => x.Email.Equals(email))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<TUser?> FindByUsername(string username)
    {
        return await _users
            .SingleOrDefaultAsync(x => x.Username.Equals(username))
            .ConfigureAwait(false);
    }

    public IAsyncEnumerable<TUser> GetDistributionListAsync()
    {
        return _users
            .AsNoTracking()
            .Where(x => x.EmailConfirmed && x.ReceiveEmails)
            .AsAsyncEnumerable();
    }

    public async Task<Guid> GetUserSecurityStampAsync(Guid userId)
    {
        return await _users
            .AsNoTracking()
            .Where(x => x.Id.Equals(userId))
            .Select(x => x.SecurityStamp)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
