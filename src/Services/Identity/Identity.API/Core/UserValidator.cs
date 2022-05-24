using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserValidator<TUser> : IUserValidator<TUser> where TUser : User
{
    public IEnumerable<IUserAttributeValidator<TUser>> UserAttributeValidators { get; }

    public UserValidator(IEnumerable<IUserAttributeValidator<TUser>> attributeValidators)
    {
        if (attributeValidators is null)
            throw new ArgumentNullException(nameof(attributeValidators));

        UserAttributeValidators = attributeValidators.OrderBy(x => x.ValidationOrder);
    }

    public async ValueTask<IdentityResult> ValidateAsync(TUser user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        IList<IdentityError> errors = new List<IdentityError>();

        foreach (var validator in UserAttributeValidators)
        {
            await validator.ValidateAsync(user, errors).ConfigureAwait(false);
        }
        return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Fail(errors);
    }
}
