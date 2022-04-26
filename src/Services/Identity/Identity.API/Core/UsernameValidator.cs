using Blog.Services.Identity.API.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class UsernameValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;
    private readonly IOptionsMonitor<UsernameOptions> _options;

    public UsernameValidator(IUserRepository<TUser> userRepository, IOptionsMonitor<UsernameOptions> options)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    public async ValueTask<IdentityResult> ValidateAsync(TUser user)
    {
        var username = user.Username;

        if (username is null || string.IsNullOrWhiteSpace(username))
            return IdentityResult.Fail(IdentityError.MissingUsername);

        var opts = _options.CurrentValue;

        if (username.Length < opts.MinLength || username.Length > opts.MaxLength ||
            (!string.IsNullOrWhiteSpace(opts.AllowedCharacters) && username.Any(x => !opts.AllowedCharacters.Contains(x))))
            return IdentityResult.Fail(IdentityError.InvalidUsernameFormat);

        var owner = await _userRepository.FindByUsername(username).ConfigureAwait(false);

        if (owner is not null)
            if (!user.Id.Equals(owner.Id))
                return IdentityResult.Fail(IdentityError.UsernameDuplicated);

        return IdentityResult.Success;
    }
}