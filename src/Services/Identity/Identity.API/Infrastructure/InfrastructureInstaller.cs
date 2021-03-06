using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure.Repositories;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;


namespace Blog.Services.Identity.API.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddCustomIdentityInfrastructure<TUser, TRole>(this IServiceCollection services, IConfiguration config)
        where TUser : User
        where TRole : Role
    {
        string connectionString = config.GetConnectionString("Postgres");

        services.AddDbContextPool<IdentityDbContext<TUser, TRole>>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddScoped<IUserRepository<TUser>, EfUserRepository<TUser, TRole>>();
        services.TryAddScoped<IUnitOfWork<TUser>, EfUnitOfWork<TUser, TRole>>();

        return services;
    }
}