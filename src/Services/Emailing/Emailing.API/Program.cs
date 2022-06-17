using Blog.Services.Emailing.API.Config;
using Blog.Services.Emailing.API.Messaging.Consumers;
using Blog.Services.Messaging.Events;
using FluentEmail.MailKitSmtp;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = GetConfiguration(env);

var services = builder.Services;

// Add services to the container.
services.AddRazorPages(opts =>
{
    opts.RootDirectory = "./Templates";
});

services
    .AddMassTransitRabbitMqBus(config)
    .AddConfiguredFluentEmail(config)
    .AddGrpc();


var app = builder.Build();

app.UseStaticFiles(); //html, css, images, js in wwwroot folder

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
            x.AddConsumersFromNamespaceContaining<SendEmailConfirmationEmailConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                services
                    .AddOptions<RabbitMqConfig>()
                    .Bind(config.GetRequiredSection(RabbitMqConfig.Section))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                var rabbitMqConfig = config.GetValue<RabbitMqConfig>(RabbitMqConfig.Section);

                cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.VirtualHost, opts =>
                {
                    opts.Username(rabbitMqConfig.Username);
                    opts.Password(rabbitMqConfig.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConfig.QueueName, opts =>
                {
                    opts.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
                    opts.ConfigureConsumers(context);
                });
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

        var emailConfig = config.GetValue<EmailConfig>(EmailConfig.Section);

        services
            .AddFluentEmail(emailConfig.FromEmail, emailConfig.FromName)
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

        return services;
    }
}

internal static class WebApplicationExtensions
{
    public static void RegisterLifetimeEvents(this WebApplication app)
    {
        IBus bus = app.Services.GetRequiredService<IBus>();
        string serviceType = "emailing-api";

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            await bus.Publish<ServiceInstanceStartedEvent>(new(ServiceType: serviceType, ServiceBaseUrls: app.Urls))
                .ConfigureAwait(false);
        });

        app.Lifetime.ApplicationStopped.Register(async () =>
        {
            await bus.Publish<ServiceInstanceStoppedEvent>(new(ServiceType: serviceType, ServiceBaseUrls: app.Urls))
            .ConfigureAwait(false);
        });
    }
}
