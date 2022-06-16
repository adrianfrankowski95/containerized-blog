
using System.Security.Claims;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface ISignInManager<TUser> where TUser : User
{
    public Task SignInAsync(TUser user, bool isPersistent);
    public Task SignOutAsync();
    public Task<bool> RefreshSignInAsync(TUser user);
    public bool IsSignedIn(ClaimsPrincipal principal);
    public bool VerifySecurityStamp(TUser? user, ClaimsPrincipal? principal);
}
