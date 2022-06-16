using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Blog.Services.Identity.API.Core;

public class SecurityStampValidator<TUser> : ISecurityStampValidator<TUser> where TUser : User
{
    private readonly UserManager<TUser> _userManager;
    private readonly ISignInManager<TUser> _signInManager;
    private readonly IOptionsMonitor<SecurityStampOptions> _securityStampOptions;
    private readonly IUserClaimsPrincipalFactory<TUser> _claimsPrincipalFactory;
    private readonly ISysTime _sysTime;

    public SecurityStampValidator(
        UserManager<TUser> userManager,
        ISignInManager<TUser> signInManager,
        IOptionsMonitor<SecurityStampOptions> securityStampOptions,
        IUserClaimsPrincipalFactory<TUser> claimsPrincipalFactory,
        ISysTime sysTime)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _securityStampOptions = securityStampOptions ?? throw new ArgumentNullException(nameof(securityStampOptions));
        _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        Instant issuedAt = context.Properties.IssuedUtc is null ?
            default : Instant.FromDateTimeOffset(context.Properties.IssuedUtc.Value);

        var interval = Duration.FromTimeSpan(_securityStampOptions.CurrentValue.SecurityStampValidationInterval);

        if (issuedAt.Plus(interval) < _sysTime.Now)
        {
            var user = await _userManager.GetUserAsync(context.Principal).ConfigureAwait(false);
            bool isSuccess = _signInManager.VerifySecurityStamp(user, context.Principal);

            if (!isSuccess || user is null)
            {
                context.RejectPrincipal();
                await _signInManager.SignOutAsync().ConfigureAwait(false);
            }
            else
            {
                var newPrincipal = await _claimsPrincipalFactory.CreateAsync(user).ConfigureAwait(false);
                context.ReplacePrincipal(newPrincipal);

                if (!context.Options.SlidingExpiration)
                {
                    context.Properties.IssuedUtc = _sysTime.Now.ToDateTimeOffset();
                }
            }
        }
    }
}
