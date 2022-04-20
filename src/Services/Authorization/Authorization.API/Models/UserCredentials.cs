namespace Blog.Services.Auth.API.Models;

public record UserCredentials(string Email, string Password, bool RememberMe);

