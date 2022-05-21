using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.API.Infrastructure.EntityConfigurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", IdentityConstants.DefaultDbSchema);

        builder.HasKey(x => x.Value);

        builder.HasIndex(x => new { x.Value, x.Name }).IsUnique();

        builder.Property(x => x.Name).IsRequired();
        builder.HasIndex(x => x.Name).HasMethod("hash").IsUnique();

        builder.HasMany<User>().WithOne(x => x.Role).IsRequired().OnDelete(DeleteBehavior.Restrict);

        builder.UseXminAsConcurrencyToken();
    }
}
