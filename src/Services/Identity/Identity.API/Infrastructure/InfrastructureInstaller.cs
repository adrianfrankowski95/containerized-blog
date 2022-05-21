using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;


namespace Blog.Services.Identity.API.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddIdentityInfrastructure<TUser>(this IServiceCollection services, IConfiguration config)
        where TUser : User
    {
        string connectionString = config.GetConnectionString("IdentityPostgresDb");

        services.AddDbContextPool<IdentityDbContext<TUser>>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddScoped<IUserRepository<TUser>, EfUserRepository<TUser>>();
        services.TryAddScoped<IUnitOfWork<TUser>, EfUnitOfWork<TUser>>();

        return services;
    }
}