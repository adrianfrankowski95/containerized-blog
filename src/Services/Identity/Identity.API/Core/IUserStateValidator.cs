using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public interface IUserStateValidator<TUser> where TUser : User
{

    public ValueTask<IdentityResult> ValidateAsync(TUser user);
}
