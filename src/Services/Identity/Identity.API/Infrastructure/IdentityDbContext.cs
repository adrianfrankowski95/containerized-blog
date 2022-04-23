using Blog.Services.Identity.API.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Models;

public class IdentityDbContext : DbContext
{
    public const string DefaultSchema = "identity";
    public DbSet<User> Users { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {

    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<NonEmptyString>()
            .HaveConversion<NonEmptyStringConverter>();

        configurationBuilder
            .Properties<NonNegativeNumber>()
            .HaveConversion<NonNegativeNumberConverter>();
    }
}