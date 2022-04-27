namespace Blog.Services.Identity.API.Core;

public interface IUserValidator<TUser> where TUser : User
{
    public IEnumerable<IUserAttributeValidator<TUser>> AttributeValidators { get; }
    public IEnumerable<IPasswordValidator<TUser>> PasswordValidators { get; }

    public ValueTask<IdentityResult> ValidateAsync(TUser user, string password);
}
