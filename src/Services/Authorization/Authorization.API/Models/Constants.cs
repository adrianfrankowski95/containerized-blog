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
    }

    public static class UserClaims
    {
        public const string Id = "user_id";
        public const string Role = "role";
        public const string Email = "email";
        public const string Name = "name";
        public const string IsPersistent = "persist";
    }
}