using System.Data.Common;
using Blog.Services.Blogging.API.Infrastructure.TypeHandlers;
using MediatR;
using Npgsql;

namespace Blog.Services.Blogging.API.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("Postgres");
        NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
        Dapper.SqlMapper.AddTypeHandler(new InstantTypeHandler());

        services
            .AddTransient<DbConnection, NpgsqlConnection>(sp => new NpgsqlConnection(connectionString))
            .AddScoped<IUserQueries, DapperUserQueries>()
            .AddMediatR(typeof(Program).Assembly);

        return services;
    }
}
