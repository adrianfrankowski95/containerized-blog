using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserStateValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    public int ValidationOrder { get; } = 2;

    public UserStateValidator(UserManager<TUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

    }

    public ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        if (_userManager.IsSuspended(user))
        {
            errors.Add(IdentityError.AccountSuspended);
        }

        if (_userManager.IsLocked(user))
        {
            errors.Add(IdentityError.AccountLocked);
        }

        if (_userManager.IsResettingPassword(user))
        {
            errors.Add(IdentityError.ResettingPassword);
        }

        return ValueTask.CompletedTask;
    }
}
