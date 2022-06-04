using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface ILoginService<TUser> where TUser : User
{
    public ValueTask<(IdentityResult result, TUser? user)> LoginAsync(string email, string password);
}
