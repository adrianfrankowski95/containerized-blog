namespace Blog.Services.Identity.API.Core;

public interface IPasswordValidator
{
    public ValueTask<IdentityResult> ValidateAsync(string password);
}
