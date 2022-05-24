
using Blog.Services.Identity.API.Core;

namespace Blog.Services.Identity.API.Services;

public interface ILoginService
{
    public ValueTask<IdentityResult> LoginAsync(string email, string password);
}
