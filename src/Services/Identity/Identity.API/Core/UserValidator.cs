using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserValidator<TUser> : IUserValidator<TUser> where TUser : User
{
    public IEnumerable<IUserAttributeValidator<TUser>> UserAttributeValidators { get; }

    public UserValidator(IEnumerable<IUserAttributeValidator<TUser>> attributeValidators)
    {
        UserAttributeValidators = attributeValidators ?? throw new ArgumentNullException(nameof(attributeValidators));
    }

    public async ValueTask<IdentityResult> ValidateAsync(TUser user)
    {
        IList<IdentityError> errors = new List<IdentityError>();

        foreach (var validator in UserAttributeValidators)
        {
            await validator.ValidateAsync(user, errors).ConfigureAwait(false);
        }
        return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Fail(errors);
    }
}
