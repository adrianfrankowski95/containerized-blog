namespace Blog.Services.Identity.API.Core;

public interface IUserValidator<TUser> where TUser : User
{
    public ICollection<IUserAttributeValidator<TUser>> AttributeValidators { get; }
    public ICollection<IPasswordValidator<TUser>> PasswordValidators { get; }

    public ValueTask<IdentityResult> ValidateAsync(TUser user, string password);
}
