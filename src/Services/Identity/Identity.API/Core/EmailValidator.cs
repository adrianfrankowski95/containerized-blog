using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class EmailValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    private readonly IOptionsMonitor<EmailOptions> _options;

    public EmailValidator(UserManager<TUser> userManager, IOptionsMonitor<EmailOptions> options)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var email = user.Email;

        if (email is null || string.IsNullOrWhiteSpace(email))
        {
            errors.Add(IdentityError.MissingEmail);
            return;
        }

        if (!new EmailAddressAttribute().IsValid(email))
            errors.Add(IdentityError.InvalidEmailFormat);

        var opts = _options.CurrentValue;

        if (opts.RequireConfirmed && _userManager.IsConfirmingEmail(user))
            errors.Add(IdentityError.EmailUnconfirmed);

        var owner = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (owner is not null && !user.Id.Equals(owner.Id))
            errors.Add(IdentityError.EmailDuplicated);

    }
}