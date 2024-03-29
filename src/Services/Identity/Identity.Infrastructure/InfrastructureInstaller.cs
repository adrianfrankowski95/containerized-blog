using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;
using Blog.Services.Identity.Infrastructure.Avatar;
using Blog.Services.Identity.Infrastructure.Idempotency;
using Blog.Services.Identity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blog.Services.Identity.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("IdentityDb")
            ?? throw new ArgumentNullException("Could not retrieve an Identity db connection string.");

        services.AddDbContextPool<IdentityDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.MigrationsAssembly("Identity.API");
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.TryAddScoped<IRequestManager, RequestManager>();
        services.TryAddScoped<IAvatarManager, AvatarManager>();
        services.TryAddScoped<IUserRepository, EfUserRepository>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddHostedService<IdentityDbMigrator>();

        return services;
    }
}