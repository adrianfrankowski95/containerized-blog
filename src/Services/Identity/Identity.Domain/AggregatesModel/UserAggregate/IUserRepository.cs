using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IUserRepository : IRepository<User>
{
    public IUnitOfWork UnitOfWork { get; }
    public Task<User?> FindUserAsync(UserId userId);
    public User AddUser(User user);
    public User DeleteUser(User user);
    public bool IsInUse(Username username);

}