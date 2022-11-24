using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Comments.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddCommentsInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("CommentsDb")
            ?? throw new ArgumentNullException("Could not retrieve comments db connection string.");

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