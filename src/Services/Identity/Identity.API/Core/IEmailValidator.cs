using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IEmailValidator : IValidator<User>
{
    public ValueTask<IdentityResult> ValidateAsync(string email);
}
