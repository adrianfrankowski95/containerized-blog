namespace Blog.Services.Identity.API.Core;

public interface IUserAttributeValidator<TUser> where TUser : User
{
    public ValueTask<IdentityResult> ValidateAsync(TUser user);
}
