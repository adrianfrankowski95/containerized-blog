namespace Blog.Services.Identity.API.Models;

public static class IdentityConstants
{
    public static class AuthenticationSchemes
    {
        public const string IdentityServiceJwt = "Blog.Identity.Jwt";
        public const string IdentityServiceCookie = "Blog.Identity.Cookie";
    }

    public static class UserClaimTypes
    {
        public const string Subject = "sub";
        public const string Role = "role";
        public const string Email = "email";
        public const string Username = "username";
        public const string FirstName = "first_name";
        public const string LastName = "last_name";
        public const string SecurityStamp = "security_stamp";
        public static string[] List() => new[] { Subject, Role, Email, Username, FirstName, LastName, SecurityStamp };
    }
}
