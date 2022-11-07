using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class OpenIddictEntityFrameworkCoreScopeConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreScope>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreScope> builder)
    {
        builder
            .ToTable("scopes", IdentityDbContext.DefaultSchema);
    }
}
