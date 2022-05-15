
using Blog.Services.Identity.API.Core;

namespace Blog.Services.Identity.API.Services;

public interface ILoginService
{
    public Task<IdentityResult> LoginAsync(HttpContext context, string email, string password, bool rememberMe);
}
