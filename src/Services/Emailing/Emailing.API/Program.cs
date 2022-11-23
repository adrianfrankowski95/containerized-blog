using Blog.Services.Emailing.API.Configs;
using Blog.Services.Emailing.API.Controllers;
using Blog.Services.Emailing.API.Factories;
using Blog.Services.Emailing.API.Grpc;
using Blog.Services.Emailing.API.Infrastructure;
using Blog.Services.Emailing.API.Services;
using FluentEmail.Core;
using FluentEmail.MailKitSmtp;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

var config = GetConfiguration(env);
builder.Configuration.AddConfiguration(config);

var services = builder.Services;

services.AddRazorPages(opts =>
{
    opts.RootDirectory = "./Templates";
});

services
    .AddInstanceConfig()
    .AddControllers(env)
    .AddEmailingInfrastructure(config)
    .AddMassTransitRabbitMqBus(config)
    .AddConfiguredFluentEmail(config)
    .AddGrpc();

var app = builder.Build();

app.UseForwardedHeaders(); //transforms x-forwarded- headers from reverse proxy to request's headers
app.UseStaticFiles(); //html, css, images, js in wwwroot folder

app.UseRouting();

app.MapGrpcService<EmailingService>();

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
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        int port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "-1");

        services.AddOptions<InstanceConfig>().Configure(opts =>
        {
            opts.InstanceId = Guid.NewGuid();
            opts.ServiceType = "emailing-api";
            opts.HeartbeatInterval = TimeSpan.FromSeconds(15);
            opts.Hostname = hostname;
            opts.Port = port;
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddMassTransitRabbitMqBus(this IServiceCollection services, IConfiguration config)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<EmailingDbContext>(cfg =>
            {
                cfg.UsePostgres();
                cfg.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = config.GetConnectionString("EventBus")
                    ?? throw new InvalidOperationException("Could not get a connection string for RabbitMq");

                cfg.Host(new Uri(connectionString));

                cfg.ReceiveEndpoint("emailing-api", opts =>
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

    public static IServiceCollection AddConfiguredFluentEmail(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<EmailConfig>()
            .Bind(config.GetRequiredSection(EmailConfig.Section))
            .Validate(x =>
            {
                if (x.RequireAuthentication)
                {
                    if (string.IsNullOrWhiteSpace(x.Password) || string.IsNullOrWhiteSpace(x.Username))
                        return false;
                }

                return Enum.TryParse<MailKit.Security.SecureSocketOptions>(x.SocketOptions, true, out _);

            }, "Invalid email configuration.")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var emailConfig = config.GetRequiredSection(EmailConfig.Section).Get<EmailConfig>();

        services
            .AddFluentEmail(emailConfig!.FromEmail, emailConfig.FromName)
            .AddRazorRenderer("./Templates")
            .AddMailKitSender(new SmtpClientOptions()
            {
                Server = emailConfig.Host,
                Port = emailConfig.Port,
                UseSsl = emailConfig.UseSsl,
                User = emailConfig.Username,
                Password = emailConfig.Password,
                RequiresAuthentication = emailConfig.RequireAuthentication,
                SocketOptions = Enum.Parse<MailKit.Security.SecureSocketOptions>(emailConfig.SocketOptions, true)
            });

        services.TryAddTransient<IEmailFactory<IFluentEmail>,
            Blog.Services.Emailing.API.Factories.FluentEmailFactory>();

        return services;
    }
}
