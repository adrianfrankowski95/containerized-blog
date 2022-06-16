using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blog.Services.Identity.API.Adapters;

public static class AdaptersInstaller
{
    public static IServiceCollection AddCustomIdentityCoreAdapters(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.TryAddTransient<ISysTime, SysTime>();
        services.TryAddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.TryAddScoped<ISignInManager<User>, SignInManager>();

        return services;
    }
}