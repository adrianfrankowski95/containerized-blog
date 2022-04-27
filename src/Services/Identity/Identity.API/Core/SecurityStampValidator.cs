using Blog.Services.Identity.API.Infrastructure.Repositories;

namespace Blog.Services.Identity.API.Core;

public class SecurityStampValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;

    public SecurityStampValidator(IUserRepository<TUser> userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var securityStamp = user.SecurityStamp;

        if (securityStamp.Equals(default))
        {
            errors.Add(IdentityError.MissingSecurityStamp);
            return;
        }

        var currentSecurityStamp = await _userRepository.GetUserSecurityStampAsync(user.Id).ConfigureAwait(false);

        if (!currentSecurityStamp.Equals(default) && !securityStamp.Equals(currentSecurityStamp))
            errors.Add(IdentityError.InvalidSecurityStamp);
    }
}
