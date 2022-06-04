
using System.Security.Claims;
using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public interface ISignInManager<TUser> where TUser : User
{
    public Task SignInAsync(HttpContext context, TUser user, bool isPersistent);
    public Task SignOutAsync(HttpContext context);
    public Task<bool> RefreshSignInAsync(HttpContext context, TUser user);
    public bool IsSignedIn(ClaimsPrincipal principal);
    public bool VerifySecurityStamp(TUser? user, ClaimsPrincipal? principal);
}
