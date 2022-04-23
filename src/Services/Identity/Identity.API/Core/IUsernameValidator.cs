using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUsernameValidator : IValidator<User>
{
    public ValueTask<IdentityResult> ValidateAsync(string username);
}
