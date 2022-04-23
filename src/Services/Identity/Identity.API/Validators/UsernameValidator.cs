using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class UsernameValidator : IUserValidator<string>
{
    private readonly UsernameOptions _options;

    public UsernameValidator(IOptionsMonitor<IdentityOptions> options)
    {
        _options = options.CurrentValue.Username ?? throw new ArgumentNullException(nameof(options));
    }
    public ValueTask<IdentityResult> ValidateAsync(string username)
    {
        throw new NotImplementedException();
    }
}
