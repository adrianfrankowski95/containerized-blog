using Microsoft.AspNetCore.Mvc.Versioning;
using NodaTime.Serialization.SystemTextJson;

namespace Blog.Gateways.WebGateway.API.Controllers;

public static class ControllersInstaller
{
    public static IServiceCollection AddGatewayControllers(this IServiceCollection services, bool isDevelopment)
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
                options.JsonSerializerOptions.WriteIndented = isDevelopment;
                options.JsonSerializerOptions.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
            });

        return services;
    }
}
