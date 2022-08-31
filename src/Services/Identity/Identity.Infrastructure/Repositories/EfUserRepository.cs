using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.Infrastructure.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly DbSet<User> _users;
    public IUnitOfWork UnitOfWork { get; }

    public EfUserRepository(IdentityDbContext ctx, IUnitOfWork unitOfWork)
    {
        _users = ctx?.Users ?? throw new ArgumentNullException(nameof(ctx));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public User AddUser(User user) => _users.Add(user).Entity;

    public User DeleteUser(User user) => _users.Remove(user).Entity;

    public ValueTask<User?> FindByIdAsync(UserId userId)
        => _users.FindAsync(userId.Value);

    public Task<User?> FindByEmailAsync(EmailAddress emailAddress)
        => _users.FirstOrDefaultAsync(x => x.EmailAddress.Equals(emailAddress));
}
