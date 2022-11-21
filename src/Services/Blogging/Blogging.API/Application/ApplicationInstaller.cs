using System.Data.Common;
using Blog.Services.Blogging.API.Application.Queries.PostQueries;
using Blog.Services.Blogging.API.Application.Queries.TagQueries;
using Blog.Services.Blogging.API.Infrastructure.TypeHandlers;
using MediatR;
using Npgsql;

namespace Blog.Services.Blogging.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("BloggingDb");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();

        Dapper.SqlMapper.AddTypeHandler(new InstantTypeHandler());

        services
            .AddTransient<DbConnection, NpgsqlConnection>(sp => new NpgsqlConnection(connectionString))
            .AddScoped<IPostQueries, DapperPostQueries>()
            .AddScoped<ITagQueries, DapperTagQueries>()
            .AddMediatR(typeof(Program).Assembly);

        return services;
    }
}
