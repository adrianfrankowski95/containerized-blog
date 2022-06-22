namespace Blog.Services.Emailing.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddHostedService<LifetimeEventsPublisher>();

        return services;
    }
}
