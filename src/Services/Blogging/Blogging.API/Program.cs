using Blog.Services.Blogging.API.Application;
using Blog.Services.Blogging.API.Configs;
using Blog.Services.Blogging.API.Controllers;
using Blog.Services.Blogging.API.Extensions;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.API.Models;
using Blog.Services.Blogging.API.Options;
using Blog.Services.Blogging.API.Services;
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
    .AddMassTransitRabbitMqBus(config)
    .AddBloggingInfrastructure(config)
    .AddBloggingApplication(config)
    .AddCustomJwtAuthentication(config)
    .AddCustomServices();

//services.AddEndpointsApiExplorer();

services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

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
        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "blogging-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }

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

    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<RabbitMqConfig>()
            .Bind(config.GetRequiredSection(RabbitMqConfig.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = config.GetRequiredSection(RabbitMqConfig.Section).Get<RabbitMqConfig>();

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, opts =>
                {
                    opts.Username(rabbitMqConfig.Username);
                    opts.Password(rabbitMqConfig.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConfig.QueueName, opts =>
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

        services.AddHostedService<RabbitMqLifetimeEventsPublisher>();

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
        services.TryAddTransient<ISysTime, SysTime>();

        return services;
    }
}