using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class OpenIddictEntityFrameworkCoreApplicationConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreApplication>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreApplication> builder)
    {
        builder
            .ToTable("applications", IdentityDbContext.DefaultSchema);
    }
}
