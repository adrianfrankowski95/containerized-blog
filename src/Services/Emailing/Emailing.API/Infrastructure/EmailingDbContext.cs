using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Emailing.API.Infrastructure;

public class EmailingDbContext : DbContext
{
    public const string DefaultSchema = "emailing";

    public EmailingDbContext(DbContextOptions<EmailingDbContext> options) : base(options)
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
