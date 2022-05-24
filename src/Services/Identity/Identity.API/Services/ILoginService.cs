
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Services;

public interface ILoginService<TUser> where TUser : User
{
    public Task<IdentityResult> LoginAsync(string email, string password, out TUser? user);
}
