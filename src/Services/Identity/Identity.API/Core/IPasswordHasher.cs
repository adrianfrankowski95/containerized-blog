namespace Blog.Services.Identity.API.Core;

public interface IPasswordHasher
{
    public PasswordVerificationResult VerifyPassword(string password, string passwordHash);
    public string HashPassword(string password);
}
