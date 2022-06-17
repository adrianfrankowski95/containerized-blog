namespace Blog.Services.Emailing.API.Models;

public enum EmailType
{
    EmailConfirmation = 1,
    PasswordReset = 2,
    NewPost = 3,
    Custom = 4
}