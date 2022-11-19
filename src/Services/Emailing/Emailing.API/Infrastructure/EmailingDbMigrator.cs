using Blog.Services.Emailing.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class EmailingDbMigrator : IHostedService
{
    private readonly IServiceProvider _services;

    public EmailingDbMigrator(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EmailingDbContext>();
            db.Database.Migrate();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}