using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Emailing.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddEmailingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Postgres");

        services.AddDbContextPool<EmailingDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        return services;
    }
}