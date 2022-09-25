using Microsoft.AspNetCore.Mvc.Versioning;
using NodaTime.Serialization.SystemTextJson;

namespace Blog.Services.Identity.API.Controllers;

public static class ControllersInstaller
{
    public static IServiceCollection AddControllers(this IServiceCollection services, IWebHostEnvironment env)
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
