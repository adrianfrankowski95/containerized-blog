using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Infrastructure.Repositories;

public interface IUserRepository
{
    public IUnitOfWork UnitOfWork { get; }
    public void Add(User user);

    public void Delete(User user);

    public void Update(User user);

    public Task<User?> FindByIdAsync(Guid userId);

    public Task<User?> FindByEmailAsync(string email);

    public Task<User?> FindByUsername(string username);

    public Task<List<User>> GetDistributionListAsync();

    public Task<Guid> GetUserSecurityStampAsync(Guid userId);
}
