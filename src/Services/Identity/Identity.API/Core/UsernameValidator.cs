using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class UsernameValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;
    private readonly IOptionsMonitor<UsernameOptions> _options;

    public int ValidationOrder { get; } = 4;

    public UsernameValidator(IUserRepository<TUser> userRepository, IOptionsMonitor<UsernameOptions> options)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    public async ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var username = user.Username;

        if (username is null || string.IsNullOrWhiteSpace(username))
        {
            errors.Add(IdentityError.MissingUsername);
            return;
        }

        var opts = _options.CurrentValue;

        if (username.Length < opts.MinLength || username.Length > opts.MaxLength ||
            (!string.IsNullOrWhiteSpace(opts.AllowedCharacters) && username.Any(x => !opts.AllowedCharacters.Contains(x))))
            errors.Add(IdentityError.InvalidUsernameFormat);

        var owner = await _userRepository.FindByUsernameAsync(username).ConfigureAwait(false);

        if (owner is not null && !user.Id.Equals(owner.Id))
            errors.Add(IdentityError.UsernameDuplicated);
    }
}