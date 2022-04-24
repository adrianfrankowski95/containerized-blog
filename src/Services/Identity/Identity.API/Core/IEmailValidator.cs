namespace Blog.Services.Identity.API.Core;

public interface IEmailValidator
{
    public ValueTask<IdentityResult> ValidateAsync(string email);
}
