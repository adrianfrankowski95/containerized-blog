using Blog.Services.Emailing.API.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using NodaTime.Serialization.SystemTextJson;

namespace Blog.Services.Emailing.API.Controllers;

public static class ControllersInstaller
{
    public static IServiceCollection AddControllers(this IServiceCollection services, IHostEnvironment env)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.UseApiBehavior = false;
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = env.IsDevelopment();
                options.JsonSerializerOptions.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
            });

        return services;
    }
}
