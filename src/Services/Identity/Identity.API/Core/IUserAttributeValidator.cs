using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUserAttributeValidator<TUser> where TUser : User
{
    public ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors);
}
