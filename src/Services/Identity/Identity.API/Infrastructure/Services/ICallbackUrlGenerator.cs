namespace Blog.Services.Identity.API.Infrastructure.Services;

public interface ICallbackUrlGenerator
{
    public string GeneratePasswordResetCallbackUrl(string passwordResetCode);
}
