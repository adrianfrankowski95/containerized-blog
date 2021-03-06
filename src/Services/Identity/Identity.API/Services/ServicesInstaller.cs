using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Blog.Services.Identity.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddTransient<ILoginService, LoginService>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<LifetimeEventsPublisher>();

        return services;
    }
}
