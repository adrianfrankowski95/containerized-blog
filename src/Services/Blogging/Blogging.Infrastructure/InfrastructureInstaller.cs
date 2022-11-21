using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Infrastructure.Idempotency;
using Blog.Services.Blogging.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blog.Services.Blogging.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddBloggingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("BloggingDb")
            ?? throw new InvalidOperationException("Could not find connection string to blogging db.");

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

        services.TryAddScoped<IRequestManager, RequestManager>();
        services.TryAddScoped<IPostRepository, EfPostRepository>();
        services.TryAddScoped<ITagRepository, EfTagRepository>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddHostedService<BloggingDbMigrator>();

        return services;
    }
}