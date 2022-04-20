namespace Blog.Services.Auth.API.Services;

public static class ServicesInstaller
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor() //required by Identity Service
            .AddSingleton<ISysTime, SysTime>()
            .AddTransient<IIdentityService, IdentityService>()
            .AddTransient<ISigningCertificateManager, SigningCertificateManager>()
            .AddScoped<ITokenManager, TokenManager>()
            .AddHostedService<UserRefreshTokenCleaner>();

        return services;
    }
}
