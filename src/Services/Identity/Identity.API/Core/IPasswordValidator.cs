namespace Blog.Services.Identity.API.Core;

public interface IPasswordValidator<TUser> where TUser : User
{
    public ValueTask<IdentityResult> ValidateAsync(TUser user, string password);
}
