using Blog.Services.Identity.API.Infrastructure.Repositories;

namespace Blog.Services.Identity.API.Core;

public class SecurityStampValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;

    public SecurityStampValidator(IUserRepository<TUser> userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async ValueTask<IdentityResult> ValidateAsync(TUser user)
    {
        var securityStamp = user.SecurityStamp;

        if (securityStamp.Equals(default))
            return IdentityResult.Fail(IdentityError.MissingSecurityStamp);

        var currentSecurityStamp = await _userRepository.GetUserSecurityStampAsync(user.Id).ConfigureAwait(false);

        return currentSecurityStamp.Equals(default) || securityStamp.Equals(currentSecurityStamp) ?
            IdentityResult.Success :
            IdentityResult.Fail(IdentityError.InvalidSecurityStamp);
    }
}
