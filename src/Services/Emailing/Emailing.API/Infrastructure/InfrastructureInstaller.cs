using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Emailing.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddEmailingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("EmailingDb")
            ?? throw new InvalidOperationException("Could not get connection string for Emailing db.");

        services.AddDbContextPool<EmailingDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.AddHostedService<EmailingDbMigrator>();

        return services;
    }
}