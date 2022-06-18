var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = GetConfiguration(env);
var services = builder.Services;

// Add services to the container.
services.AddLogging();
services.AddControllers();
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

app.RegisterLifetimeEvents();

app.Run();


static IConfiguration GetConfiguration(IWebHostEnvironment env)
    => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

internal static class WebApplicationExtensions
{
    public static void RegisterLifetimeEvents(this WebApplication app)
    {
        IBus bus = app.Services.GetRequiredService<IBus>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

        string serviceType = "CommentsApi";

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