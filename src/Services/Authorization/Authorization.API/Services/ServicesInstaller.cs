using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blog.Services.Auth.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor() //required by Identity Service
            .TryAddTransient<IIdentityService, IdentityService>();

        return services;
    }
}
