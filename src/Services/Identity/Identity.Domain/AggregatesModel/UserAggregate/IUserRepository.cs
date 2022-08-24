using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IUserRepository : IRepository<User>
{
    public IUnitOfWork UnitOfWork { get; }
    public Task<User?> FindByIdAsync(UserId userId);
    public Task<User?> FindByUsernameAsync(Username username);
    public User AddUser(User user);
    public User DeleteUser(User user);
}