namespace Blog.Services.Identity.API.Services;

public interface IPasswordHasher
{
    public bool VerifyPassword(string password, string passwordHash);
    public string HashPassword(string password);
    public bool PasswordNeedsRehash(string passwordHash);
}
