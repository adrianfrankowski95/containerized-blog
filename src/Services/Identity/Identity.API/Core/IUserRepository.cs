using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUserRepository<TUser> where TUser : User
{
    public void Add(TUser user);

    public void Remove(TUser user);

    public void Update(TUser user);

    public Task<TUser?> FindByIdAsync(Guid userId);

    public Task<TUser?> FindByEmailAsync(string email);

    public Task<TUser?> FindByUsernameAsync(string username);

    public IAsyncEnumerable<TUser> GetDistributionListAsync();

    public Task<Guid> GetSecurityStampAsync(Guid userId);
}
