using Blog.Services.Blogging.Infrastructure;
using Blog.Services.Blogging.API.Application;
using Blog.Services.Blogging.API.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Blog.Services.Blogging.API.Application.Models;
using Microsoft.IdentityModel.Tokens;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Blog.Services.Blogging.API.Extensions;
using NodaTime;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using SysTime = Blog.Services.Blogging.API.Infrastructure.Services.SysTime;
using Blog.Services.Blogging.API.Options;

var builder = WebApplication.CreateBuilder(args);

bool isDevelopment = builder.Environment.IsDevelopment();
var config = GetConfiguration(isDevelopment);

var services = builder.Services;

// Add services to the container.

services
    .AddBloggingControllers(isDevelopment)
    .AddNodaTime()
    .AddBloggingInfrastructure(config)
    .AddBloggingApplication(config)
    .AddCustomJwtAuthentication(config);

//services.AddEndpointsApiExplorer();

services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseGlobalExceptionHandler(); //custom global error handling

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//await BloggingContextSeed.SeedAsync(connectionString);
//var summary = BenchmarkRunner.Run<EfPostQueriesBenchmark>();

app.Run();

static IConfiguration GetConfiguration(bool isDevelopment)
{
    string configFileName = isDevelopment ? "appsettings.Development.json" : "appsettings.json";

    return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileName, optional: false, reloadOnChange: true)
                .Build();
}
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtOptions>()
            .Bind(config.GetRequiredSection(JwtOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtConfig = config.GetRequiredSection(JwtOptions.Section);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                opts.MapInboundClaims = false;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = false,
                    ValidateIssuerSigningKey = false,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtConfig.GetValue<string>("Issuer"),
                    ValidAudience = jwtConfig.GetValue<string>("Audience"),
                    NameClaimType = UserClaimTypes.Name,
                    RoleClaimType = UserClaimTypes.Role,
                    AuthenticationType = JwtBearerDefaults.AuthenticationScheme,
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            .TryAddTransient<IIdentityService, IdentityService>();

        return services;
    }

    public static IServiceCollection AddNodaTime(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddTransient<ISysTime, SysTime>();

        return services;
    }
}