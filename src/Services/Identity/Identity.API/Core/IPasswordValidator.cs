namespace Blog.Services.Identity.API.Core;

public interface IPasswordValidator<TUser> where TUser : User
{
    public ValueTask ValidateAsync(TUser user, string password, ICollection<IdentityError> errors);
}
