using Blog.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blog.Services.Identity.API.Core;

public static class CoreInstaller
{
    public static IServiceCollection AddCustomIdentityCore<TUser>(this IServiceCollection services) where TUser : User
    {
        services
            .AddAuthentication(IdentityConstants.AuthenticationScheme)
            .AddCookie(IdentityConstants.AuthenticationScheme, opts =>
            {
                opts.Cookie.HttpOnly = true;
                opts.Cookie.IsEssential = true;
                opts.Cookie.SameSite = SameSiteMode.Strict;
                opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                opts.LoginPath = new PathString("/Account/Login");
            }).Services
            .AddOptions<CookieAuthenticationOptions>(IdentityConstants.AuthenticationScheme)
            .Configure<ISecurityStampValidator<TUser>>((opts, validator)
                => opts.Events = new() { OnValidatePrincipal = validator.ValidateAsync });

        services.AddOptions<EmailOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<UsernameOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<PasswordOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<LockoutOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<SecurityStampOptions>().ValidateDataAnnotations().ValidateOnStart();

        services.TryAddScoped<IUserAttributeValidator<TUser>, EmailValidator<TUser>>();
        services.TryAddScoped<IUserAttributeValidator<TUser>, UsernameValidator<TUser>>();
        services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
        services.TryAddScoped<ISecurityStampValidator<TUser>, SecurityStampValidator<TUser>>();
        services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
        services.TryAddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();

        services.TryAddScoped<ILoginService<TUser>, LoginService<TUser>>();
        services.TryAddScoped<ISignInManager<TUser>, SignInManager<TUser>>();
        services.TryAddScoped<UserManager<TUser>>();

        return services;
    }
}
