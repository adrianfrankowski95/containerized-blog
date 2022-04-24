namespace Blog.Services.Identity.API.Core;

public interface IUserValidator
{
    public ValueTask<IdentityResult> ValidateAsync(IUser user);
}
