using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Blog.Services.Identity.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddTransient<ISysTime, SysTime>();

        return services;
    }
}
