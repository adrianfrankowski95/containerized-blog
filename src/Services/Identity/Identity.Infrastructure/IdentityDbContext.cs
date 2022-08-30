using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Blog.Services.Identity.Infrastructure;

public class IdentityDbContext : DbContext
{
    public const string DefaultSchema = "identity";
    public DbSet<User> Users { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<NonEmptyString>().HaveConversion<string>();
        configurationBuilder.Properties<NonNegativeInt>().HaveConversion<int>();
        configurationBuilder.Properties<NonPastInstant>().HaveConversion<Instant>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
