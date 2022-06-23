using System.Data.Common;
using Blog.Services.Blogging.API.Application.Commands;
using Blog.Services.Blogging.API.Application.Queries.PostQueries;
using Blog.Services.Blogging.API.Application.Queries.TagQueries;
using Blog.Services.Blogging.API.Infrastructure.TypeHandlers;
using MediatR;
using Npgsql;

namespace Blog.Services.Blogging.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddBloggingApplication(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Postgres");
        NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
        Dapper.SqlMapper.AddTypeHandler(new InstantTypeHandler());

        services
            .AddTransient<DbConnection, NpgsqlConnection>(sp => new NpgsqlConnection(connectionString))
            .AddScoped<IPostQueries, DapperPostQueries>()
            .AddScoped<ITagQueries, DapperTagQueries>()
            .AddMediatR(typeof(Program).Assembly);

        return services;
    }
}
