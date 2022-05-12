using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Authorization.API.Infrastructure.EntityConfigurations;

public static class AuthContextSeed
{
    public async static Task SeedAsync(IConfiguration config)
    {
        string connectionString = config.GetConnectionString("AuthPostgresDb");

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(connectionString, opt =>
            {
                opt.UseNodaTime();
            })
            .UseSnakeCaseNamingConvention()
            .Options;


        await using (var context = new AuthDbContext(options))
        {
            context.Database.Migrate();
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
