namespace Blog.Services.Identity.API.Core;

public interface IUsernameValidator
{
    public ValueTask<IdentityResult> ValidateAsync(string username);
}
