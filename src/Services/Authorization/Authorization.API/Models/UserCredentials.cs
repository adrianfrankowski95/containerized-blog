namespace Blog.Services.Authorization.API.Models;

public record UserCredentials(string Email, string Password, bool RememberMe);

