using Blog.Services.Identity.API.Infrastructure.EntityConfigurations;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Infrastructure;

public class IdentityDbContext<TUser, TRole> : DbContext
    where TUser : User
    where TRole : UserRole
{
    public DbSet<TUser> Users { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext<TUser, TRole>> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new UserEntityConfiguration<TUser>())
            .ApplyConfiguration(new RoleEntityConfiguration<TRole>());
    }
}