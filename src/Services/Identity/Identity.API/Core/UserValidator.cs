using Blog.Services.Identity.API.Models;

namespace Blog.Services.Identity.API.Core;

public class UserValidator<TUser> : IUserValidator<TUser> where TUser : UserBase
{
    public IEnumerable<IUserAttributeValidator<TUser>> AttributeValidators { get; }
    public IEnumerable<IPasswordValidator<TUser>> PasswordValidators { get; }

    public UserValidator(
        IEnumerable<IUserAttributeValidator<TUser>> attributeValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators)
    {
        AttributeValidators = attributeValidators ?? throw new ArgumentNullException(nameof(attributeValidators));
        PasswordValidators = passwordValidators ?? throw new ArgumentNullException(nameof(passwordValidators));
    }

    public async ValueTask<IdentityResult> ValidateAsync(TUser user, string password)
    {
        IList<IdentityError> errors = new List<IdentityError>();

        foreach (var validator in AttributeValidators)
        {
            await validator.ValidateAsync(user, errors).ConfigureAwait(false);
        }

        foreach (var validator in PasswordValidators)
        {
            await validator.ValidateAsync(user, password, errors).ConfigureAwait(false);
        }

        return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Fail(errors);
    }
}
