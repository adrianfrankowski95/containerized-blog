using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Discovery.API.Infrastructure;

public class DiscoveryDbContext : DbContext
{
    public const string DefaultSchema = "discovery";

    public DiscoveryDbContext(DbContextOptions<DiscoveryDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mass transit EF Core outbox configurations
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state", "outbox"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("message", "outbox"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state", "outbox"));
    }
}
