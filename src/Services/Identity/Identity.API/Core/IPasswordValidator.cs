using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IPasswordValidator<TUser> where TUser : UserBase
{
    public ValueTask ValidateAsync(TUser user, string password, ICollection<IdentityError> errors);
}
