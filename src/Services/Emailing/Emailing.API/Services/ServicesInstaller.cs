namespace Blog.Services.Emailing.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<LifetimeEventsPublisher>();

        return services;
    }
}
