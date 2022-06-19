using Blog.Services.Identity.API.Adapters;
using Blog.Services.Identity.API.Configs;
using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Blog.Services.Integration.Events;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = GetConfiguration(env);

var services = builder.Services;

// Add services to the container.
services.AddLogging();
services.AddRazorPages();

services
    .AddCustomIdentityInfrastructure<User, Role>(config)
    .AddCustomIdentityCore<User>()
    .AddCustomIdentityCoreAdapters()
    .AddCustomServices()
    .AddMassTransitRabbitMqBus(config);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); //html, css, images, js in wwwroot folder

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.RegisterLifetimeEvents();

app.Run();


static IConfiguration GetConfiguration(IWebHostEnvironment env)
    => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = config.GetValue<RabbitMqConfig>(RabbitMqConfig.Section);

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.VirtualHost, opts =>
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
            });

        return services;
    }
}

internal static class WebApplicationExtensions
{
    public static void RegisterLifetimeEvents(this WebApplication app)
    {
        IBus bus = app.Services.GetRequiredService<IBus>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

        string serviceType = "IdentityApi";

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            logger.LogInformation("----- Service started: {Type} - {Urls}", serviceType, string.Join(',', app.Urls));
            await bus.Publish<ServiceInstanceStartedEvent>(new(ServiceType: serviceType, ServiceUrls: app.Urls))
                .ConfigureAwait(false);
        });

        app.Lifetime.ApplicationStopped.Register(async () =>
        {
            logger.LogInformation("----- Service stopped: {Type} - {Urls}", serviceType, string.Join(',', app.Urls));
            await bus.Publish<ServiceInstanceStoppedEvent>(new(ServiceType: serviceType, ServiceUrls: app.Urls))
                .ConfigureAwait(false);
        });
    }
}