using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Infrastructure.Repositories;

public interface IUserRepository<TUser> where TUser : UserBase
{
    public void Add(TUser user);

    public void Delete(TUser user);

    public void Update(TUser user);

    public Task<TUser?> FindByIdAsync(Guid userId);

    public Task<IList<TUser>> FindByEmailAsync(string email);

    public Task<TUser?> FindByUsername(string username);

    public IAsyncEnumerable<TUser> GetDistributionListAsync();

    public Task<Guid> GetUserSecurityStampAsync(Guid userId);
}
