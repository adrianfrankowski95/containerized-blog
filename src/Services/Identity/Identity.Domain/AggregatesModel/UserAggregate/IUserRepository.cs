using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IUserRepository : IRepository<User>
{
    public IUnitOfWork UnitOfWork { get; }
    public ValueTask<User?> FindByIdAsync(UserId userId);
    public Task<User?> FindByEmailAsync(EmailAddress emailAddress);
    public Task<User?> FindByUsernameAsync(NonEmptyString username);
    public User AddUser(User user);
    public User DeleteUser(User user);
}