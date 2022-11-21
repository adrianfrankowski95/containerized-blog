using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Comments.API.Infrastructure;

public class CommentsDbMigrator : IHostedService
{
    private readonly IServiceProvider _services;

    public CommentsDbMigrator(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<CommentsDbContext>();
            ctx.Database.Migrate();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}