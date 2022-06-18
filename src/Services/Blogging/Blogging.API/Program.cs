using Blog.Services.Blogging.API.Application;
using Blog.Services.Blogging.API.Controllers;
using Blog.Services.Blogging.API.Extensions;
using Blog.Services.Blogging.API.Infrastructure.Services;
using Blog.Services.Blogging.API.Models;
using Blog.Services.Blogging.API.Options;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using SysTime = Blog.Services.Blogging.API.Infrastructure.Services.SysTime;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var config = GetConfiguration(env);

var services = builder.Services;

// Add services to the container.
services.AddLogging();
services
    .AddBloggingControllers(env.IsDevelopment())
    .AddNodaTime()
    .AddBloggingInfrastructure(config)
    .AddBloggingApplication(config)
    .AddCustomJwtAuthentication(config);

//services.AddEndpointsApiExplorer();

services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (env.IsDevelopment())
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
app.RegisterLifetimeEvents();

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

internal static class WebApplicationExtensions
{
    public static void RegisterLifetimeEvents(this WebApplication app)
    {
        IBus bus = app.Services.GetRequiredService<IBus>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

        string serviceType = "BloggingApi";

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            logger.LogInformation("----- Service started: {Type} - {Urls}", serviceType, string.Join(',', app.Urls));
            await bus.Publish<ServiceInstanceStartedEvent>(new(ServiceType: serviceType, ServiceBaseUrls: app.Urls))
                .ConfigureAwait(false);
        });

        app.Lifetime.ApplicationStopped.Register(async () =>
        {
            logger.LogInformation("----- Service stopped: {Type} - {Urls}", serviceType, string.Join(',', app.Urls));
            await bus.Publish<ServiceInstanceStoppedEvent>(new(ServiceType: serviceType, ServiceBaseUrls: app.Urls))
            .ConfigureAwait(false);
        });
    }
}