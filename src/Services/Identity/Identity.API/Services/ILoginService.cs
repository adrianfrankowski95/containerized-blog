using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Services;

public interface ILoginService
{
    public ValueTask<(IdentityResult result, User? user)> LoginAsync(string email, string password);
}