using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface ILoginService
{
    public Task<LoginResult> LoginAsync(EmailAddress providedEmailAddress, PasswordHasher.PasswordHash providedPasswordHash);
}