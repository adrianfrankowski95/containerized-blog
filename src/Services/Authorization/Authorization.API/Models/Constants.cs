namespace Blog.Services.Authorization.API.Models;

public static class Constants
{
    public static class Scopes
    {
        public const string BloggingApi = "blog.blogging_api";
        public const string EmailingApi = "blog.emailing_api";
        public const string UsersApi = "blog.users_api";
        public const string CommentsApi = "blog.comments_api";
        public const string AuthApi = "blog.auth_api";
        public static string[] List() => new[] { BloggingApi, EmailingApi, UsersApi, CommentsApi, AuthApi };
    }

    public static class UserClaimTypes
    {
        public const string Id = "sub";
        public const string Role = "role";
        public const string Email = "email";
        public const string EmailConfirmed = "email_confirmed";
        public const string Name = "name";
        public const string SecurityStamp = "security_stamp";
        public const string IsPersistent = "persist";
        public static string[] List() => new[] { Id, Role, Email, EmailConfirmed, Name, SecurityStamp, IsPersistent };
    }

    public static class UserRoleTypes
    {
        public const string Reader = "reader";
        public const string Blogger = "blogger";
        public const string Moderator = "moderator";
        public const string Administrator = "administrator";
        public static string[] List() => new[] { Reader, Blogger, Moderator, Administrator };
    }
}