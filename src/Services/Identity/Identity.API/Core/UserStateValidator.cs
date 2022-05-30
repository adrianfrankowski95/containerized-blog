using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserStateValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    public int ValidationOrder { get; } = 1;

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
            errors.Add(UserStateValidationError.AccountSuspended);
        }

        if (_userManager.IsLockedOut(user))
        {
            errors.Add(UserStateValidationError.AccountLockedOut);
        }

        if (_userManager.IsResettingPassword(user))
        {
            errors.Add(UserStateValidationError.ResettingPassword);
        }

        return ValueTask.CompletedTask;
    }
}
