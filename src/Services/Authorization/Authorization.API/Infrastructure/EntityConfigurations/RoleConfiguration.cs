using Blog.Services.Authorization.API.Models;
;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Authorization.API.Infrastructure.EntityConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder
            .ToTable("roles", AuthDbContext.DefaultSchema);

        builder
            .HasIndex(x => x.Value).IsUnique();

        builder
            .HasIndex(x => x.Name).IsUnique();
    }
}
