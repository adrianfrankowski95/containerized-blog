
using Blog.Services.Comments.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Discovery.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddCommentsInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Postgres");

        services.AddDbContextPool<CommentsDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.AddHostedService<CommentsDbMigrator>();

        return services;
    }
}