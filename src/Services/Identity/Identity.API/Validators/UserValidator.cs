using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class UserValidator : IUserValidator<User, User>
{
    private readonly IUserRepository _userRepository;
    private readonly IOptionsMonitor<IdentityOptions> _options;

    public UserValidator(IUserRepository userRepository, IOptionsMonitor<IdentityOptions> options)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public ValueTask<IdentityResult> ValidateAsync(User attribute)
    {
        throw new NotImplementedException();
    }
}
