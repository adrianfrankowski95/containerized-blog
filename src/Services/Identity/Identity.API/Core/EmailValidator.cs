using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class EmailValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    public int ValidationOrder { get; } = 1;

    public EmailValidator(UserManager<TUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var email = user.EmailAddress;

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(EmailValidationError.MissingEmail);
            return;
        }

        if (!new EmailAddressAttribute().IsValid(email))
            errors.Add(EmailValidationError.InvalidEmailFormat);

        var owner = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (owner is not null && !user.Id.Equals(owner.Id))
            errors.Add(EmailValidationError.EmailDuplicated);

    }
}