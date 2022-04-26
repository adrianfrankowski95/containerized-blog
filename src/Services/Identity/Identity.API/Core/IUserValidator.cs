namespace Blog.Services.Identity.API.Core;

public interface IUserValidator<TUser> where TUser : User
{
    public IEnumerable<IUserAttributeValidator<TUser>> AttributeValidators { get; }
    public IPasswordValidator<TUser> PasswordValidator { get; }
}
