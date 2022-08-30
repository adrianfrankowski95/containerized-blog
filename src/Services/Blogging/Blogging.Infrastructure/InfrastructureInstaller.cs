using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Infrastructure.Idempotency;
using Blog.Services.Blogging.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using SysTime = Blog.Services.Blogging.Infrastructure.Services.SysTime;

namespace Blog.Services.Blogging.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddBloggingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Postgres");

        services.AddDbContextPool<BloggingDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.MigrationsAssembly("Blogging.API");
                opts.EnableRetryOnFailure();
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddTransient<ISysTime, SysTime>();
        services.TryAddScoped<IRequestManager, RequestManager>();
        services.TryAddScoped<IPostRepository, EfPostRepository>();
        services.TryAddScoped<ITagRepository, EfTagRepository>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}