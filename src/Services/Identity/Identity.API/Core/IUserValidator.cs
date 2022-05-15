using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface IUserValidator<TUser> where TUser : User
{
    public IEnumerable<IUserAttributeValidator<TUser>> UserAttributeValidators { get; }

    public ValueTask<IdentityResult> ValidateAsync(TUser user);
}
