using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.Extensions.Options;

namespace Blog.Services.Identity.API.Core;

public class UserStatusValidator<TUser> : IUserAttributeValidator<TUser> where TUser : User
{
    private readonly IOptionsMonitor<SecurityOptions> _options;
    private readonly ISysTime _sysTime;

    public UserStatusValidator(IOptionsMonitor<SecurityOptions> options, ISysTime sysTime)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public ValueTask ValidateAsync(TUser user, ICollection<IdentityError> errors)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        var opts = _options.CurrentValue;

        if (user.SuspendedUntil is not null && user.SuspendedUntil > _sysTime.Now)
        {
            errors.Add(IdentityError.AccountSuspended);
        }

        if (opts.EnableLoginAttemptsLock && user.LockedUntil is not null && user.LockedUntil > _sysTime.Now)
        {
            errors.Add(IdentityError.AccountLocked);
        }

        if (user.PasswordResetCode is not null)
        {
            errors.Add(IdentityError.ResettingPassword);
        }

        return ValueTask.CompletedTask;
    }
}
