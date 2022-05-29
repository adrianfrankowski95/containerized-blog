using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Blog.Services.Identity.API.Core;

public interface ISecurityStampValidator<TUser> where TUser : User
{
    public Task ValidateAsync(CookieValidatePrincipalContext context);
}
