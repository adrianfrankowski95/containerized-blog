
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Services;

public interface ISignInManager<TUser> where TUser : User
{
    public Task SignInAsync(HttpContext context, TUser user, bool isPersistent);
    public Task SignOutAsync(HttpContext context);
}
