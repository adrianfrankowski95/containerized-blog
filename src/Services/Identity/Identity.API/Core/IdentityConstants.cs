namespace Blog.Services.Identity.API.Core;

public static class IdentityConstants
{
    public const string AuthenticationScheme = "blog.identity";
    public static class ClaimTypes
    {
        public const string Id = "sub";
        public const string Username = "username";
        public const string Email = "email";
        public const string EmailConfirmed = "email_confirmed";
        public const string Role = "role";
        public const string Language = "language";
        public const string SecurityStamp = "security_stamp";
    }
}
