using Blog.Services.Authorization.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Authorization.API.Infrastructure.EntityConfigurations;

public class AuthDbContext : IdentityDbContext<
    User,
    Role,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    public const string DefaultSchema = "identity";
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
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreApplicationConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreAuthorizationConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreScopeConfiguration())
            .ApplyConfiguration(new OpenIddictEntityFrameworkCoreTokenConfiguration())

            .ApplyConfiguration(new UserConfiguration())
            .ApplyConfiguration(new RoleConfiguration())
            .ApplyConfiguration(new UserRoleConfiguration())
            .ApplyConfiguration(new UserClaimConfiguration())
            .ApplyConfiguration(new UserTokenConfiguration())
            .ApplyConfiguration(new UserLoginConfiguration())
            .ApplyConfiguration(new RoleClaimConfiguration());
    }
}
