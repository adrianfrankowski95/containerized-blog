namespace Blog.Services.Identity.API.Core;

public interface IPasswordHasher
{
    public bool VerifyPassword(string password, string passwordHash);
    public string HashPassword(string password);
    public bool CheckPasswordNeedsRehash(string passwordHash);
}
