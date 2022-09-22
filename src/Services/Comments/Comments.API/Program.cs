using Blog.Services.Comments.API.Configs;
using Blog.Services.Comments.API.Controllers;
using Blog.Services.Comments.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

var config = GetConfiguration(env);
builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services
    .AddInstanceConfig()
    .AddControllers(env)
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

app.UseAuthorization();

app.MapControllers();

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
    public static IServiceCollection AddInstanceConfig(this IServiceCollection services)
    {
        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "comments-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

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
                    opts.ConfigureConsumers(context);
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
}