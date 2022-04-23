using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Infrastructure.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly DbSet<User> _users;
    public IUnitOfWork UnitOfWork { get; }

    public EfUserRepository(IdentityDbContext ctx, IUnitOfWork unitOfWork)
    {
        _users = ctx.Users ?? throw new ArgumentNullException(nameof(ctx));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public void Add(User user)
    {
        _users.Add(user);
    }

    public void Delete(User user)
    {
        _users.Remove(user);
    }

    public void Update(User user)
    {
        _users.Update(user);
    }

    public async Task<User?> FindByIdAsync(Guid userId)
    {
        return await _users
            .FindAsync(userId)
            .ConfigureAwait(false);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _users
            .SingleOrDefaultAsync(x => x.Email.Equals(email))
            .ConfigureAwait(false);
    }

    public async Task<User?> FindByUsername(string username)
    {
        return await _users
            .SingleOrDefaultAsync(x => x.Username.Equals(username))
            .ConfigureAwait(false);
    }

    public async Task<List<User>> GetDistributionListAsync()
    {
        return await _users
            .AsNoTracking()
            .Where(x => x.EmailConfirmed && x.ReceiveEmails)
            .ToListAsync()
            .ConfigureAwait(false);
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
