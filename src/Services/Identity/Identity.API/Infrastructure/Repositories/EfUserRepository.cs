using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Infrastructure.Repositories;

public class EfUserRepository<TUser, TRole> : IUserRepository<TUser>
    where TUser : User
    where TRole : Role
{
    private readonly DbSet<TUser> _users;

    public EfUserRepository(IdentityDbContext<TUser, TRole> ctx)
    {
        _users = ctx.Users ?? throw new ArgumentNullException(nameof(ctx));
    }

    public void Add(TUser user)
    {
        _users.Add(user);
    }

    public void Remove(TUser user)
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

    public async Task<TUser?> FindByEmailAsync(string email)
    {
        return await _users
            .SingleOrDefaultAsync(x => x.EmailAddress.Equals(email))
            .ConfigureAwait(false);
    }

    public async Task<TUser?> FindByUsernameAsync(string username)
    {
        return await _users
            .SingleOrDefaultAsync(x => x.Username.Equals(username))
            .ConfigureAwait(false);
    }

    public IAsyncEnumerable<TUser> GetDistributionListAsync()
    {
        return _users
            .AsNoTracking()
            .Where(x => x.EmailConfirmed && x.ReceiveAdditionalEmails)
            .AsAsyncEnumerable();
    }

    public async Task<Guid> GetSecurityStampAsync(Guid userId)
    {
        return await _users
            .AsNoTracking()
            .Where(x => x.Id.Equals(userId))
            .Select(x => x.SecurityStamp)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
