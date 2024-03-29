using Blog.Services.Blogging.API.Application;
using Blog.Services.Blogging.API.Configs;
using Blog.Services.Blogging.API.Controllers;
using Blog.Services.Blogging.API.Extensions;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.API.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using SysTime = Blog.Services.Blogging.API.Infrastructure.Services.SysTime;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = GetConfiguration(env);

builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services
    .AddInstanceConfig()
    .AddControllers(env)
    .AddNodaTime()
    .AddCustomServices()
    .AddDomainServices()
    .AddApplicationServices(config)
    .AddBloggingInfrastructure(config)
    .AddMassTransitRabbitMqBus(config)
    .AddCustomJwtAuthentication(config);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseGlobalExceptionHandler();

app.UseForwardedHeaders(); //transforms x-forwarded- headers from reverse proxy to request's headers

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//await BloggingContextSeed.SeedAsync(connectionString);

app.Run();

static IConfiguration GetConfiguration(IWebHostEnvironment env)
    => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstanceConfig(this IServiceCollection services)
    {
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        int port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "-1");

        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "blogging-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
            opts.Hostname = hostname;
            opts.Port = port;
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtConfig>()
            .Bind(config.GetRequiredSection(JwtConfig.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtConfig = config.GetRequiredSection(JwtConfig.Section);

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

    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<BloggingDbContext>(cfg =>
            {
                cfg.UsePostgres();
                cfg.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = config.GetConnectionString("EventBus")
                    ?? throw new InvalidOperationException("Could not get a connection string for RabbitMq");

                cfg.Host(new Uri(connectionString));
                cfg.ReceiveEndpoint("blogging-api", opts =>
                {
                    opts.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
                });

                cfg.Durable = true;
            });
        });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(opts =>
            {
                opts.WaitUntilStarted = true;
                opts.StartTimeout = TimeSpan.FromSeconds(10);
                opts.StopTimeout = TimeSpan.FromSeconds(30);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<RabbitMqLifetimeIntegrationEventsPublisher>();

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
        services.TryAddSingleton<IClock>(SystemClock.Instance);
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.TryAddTransient<ISysTime, SysTime>();
        return services;
    }
}