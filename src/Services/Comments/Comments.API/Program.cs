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

        Guid instanceId = Guid.NewGuid();
        string serviceType = "comments-api";
        string urlsString = string.Join("; ", app.Urls);

        app.Lifetime.ApplicationStarted.Register(async () =>
        {
            logger.LogInformation("----- {Type} service instance started: {Id} - {Urls}", serviceType, instanceId, urlsString);
            await bus.Publish<ServiceInstanceStartedEvent>(new(instanceId, serviceType, app.Urls))
                .ConfigureAwait(false);
        });

        app.Lifetime.ApplicationStopped.Register(async () =>
        {
            logger.LogInformation("----- {Type} service instance stopped: {Id} - {Urls}", serviceType, instanceId, urlsString);
            await bus.Publish<ServiceInstanceStoppedEvent>(new(instanceId, serviceType, app.Urls))
                .ConfigureAwait(false);
        });
    }
}