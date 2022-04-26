using System.ComponentModel.DataAnnotations;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class EmailValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IUserRepository<TUser> _userRepository;
    private readonly IOptionsMonitor<EmailOptions> _options;

    public EmailValidator(IUserRepository<TUser> userRepository, IOptionsMonitor<EmailOptions> options)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        if (opts.RequireConfirmed && !user.EmailConfirmed)
            errors.Add(IdentityError.EmailUnconfirmed);

        if (opts.RequireUnique)
        {
            var owners = await _userRepository.FindByEmailAsync(email).ConfigureAwait(false);

            if (owners is not null && owners.Count > 0)
            {
                //if there is only 1 owner, check if this is a provided user
                if (owners.Count == 1)
                {
                    var owner = owners.First();

                    if (!user.Id.Equals(owner.Id))
                        errors.Add(IdentityError.EmailDuplicated);
                }
                else
                    errors.Add(IdentityError.EmailDuplicated);
            }
        }
    }
}