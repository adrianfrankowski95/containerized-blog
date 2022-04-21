using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Blog.Services.Identity.API.Models;

public class IdentityDbContext : DbContext
{
    public const string DefaultSchema = "identity";
    public DbSet<User> Users { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {

    }
}