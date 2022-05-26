
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Services;

public interface ILoginService<TUser> where TUser : User
{
    public Task<(IdentityResult result, TUser? user)> LoginAsync(string email, string password);
}
