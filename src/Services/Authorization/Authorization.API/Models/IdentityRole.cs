using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Authorization.API.Models;

public class IdentityRole : IdentityRole<int>
{
    public IdentityRole(UserRole role) : base(role.ToString())
    {
        Id = (int)role;
    }
}