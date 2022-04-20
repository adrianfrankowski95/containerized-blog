using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Infrastructure.Idempotency;
using Blog.Services.Blogging.Infrastructure.Repositories;
using Blog.Services.Blogging.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SysTime = Blog.Services.Blogging.Infrastructure.Services.SysTime;

namespace Blog.Services.Blogging.Infrastructure;


public static class InfrastructureInstaller
{
    public static IServiceCollection AddBloggingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("BloggingPostgresDb");

        services.AddDbContextPool<BloggingDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString, opts =>
            {
                opts.UseNodaTime();
                opts.MigrationsAssembly("Blogging.API");
            });
            opts.UseSnakeCaseNamingConvention();
        });

        services
            .AddSingleton<ISysTime, SysTime>()
            .AddScoped<IRequestManager, RequestManager>()
            .AddScoped<IPostRepository, EfPostRepository>()
            .AddScoped<ITagRepository, EfTagRepository>()
            .AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}