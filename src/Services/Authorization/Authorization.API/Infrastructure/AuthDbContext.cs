using Blog.Services.Auth.API.Infrastructure.EntityConfigurations;
using Blog.Services.Auth.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Auth.API.Infrastructure;

public class AuthDbContext : DbContext
{
    public const string DefaultSchema = "auth";
    //public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    //public AuthContext() : base() { }
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     string connectionString = "host=localhost;port=5432;database=auth;password=adrianek;username=postgres";
    //     optionsBuilder.UseNpgsql(connectionString, opt =>
    //         {
    //             opt.UseNodaTime();
    //         })
    //         .UseSnakeCaseNamingConvention();
    //     base.OnConfiguring(optionsBuilder);
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfiguration(new UserRefreshTokenEntityConfiguration());
        modelBuilder
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreApplicationConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreAuthorizationConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreScopeConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreTokenConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
