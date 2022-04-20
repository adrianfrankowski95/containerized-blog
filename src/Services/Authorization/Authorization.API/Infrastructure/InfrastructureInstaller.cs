using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Auth.API.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection AddInfrastructureForOpenIddict(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("AuthPostgresDb");

        // services.AddDbContextPool<AuthDbContext>(opts =>
        // {
        //     opts.UseNpgsql(connectionString, opts =>
        //     {
        //         opts.UseNodaTime();
        //         opts.MigrationsAssembly("Authorization.API");
        //     });
        //     opts.UseSnakeCaseNamingConvention();
        // });

        // services
        //     .AddScoped<IUnitOfWork, EfUnitOfWork>()
        //     .AddScoped<IUserRefreshTokenRepository, EfUserRefreshTokenRepository>();

        services.AddDbContextPool<AuthDbContext>(opts =>
        {
            opts
                .UseNpgsql(connectionString, opts =>
                    {
                        opts.UseNodaTime();
                        opts.MigrationsAssembly("Authorization.API");
                    })
                .UseOpenIddict<Guid>()
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
