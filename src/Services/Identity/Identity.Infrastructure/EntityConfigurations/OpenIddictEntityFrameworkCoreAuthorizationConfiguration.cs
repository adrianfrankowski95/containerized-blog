using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class OpenIddictEntityFrameworkCoreAuthorizationConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreAuthorization>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreAuthorization> builder)
    {
        builder
            .ToTable("authorizations", IdentityDbContext.DefaultSchema);
    }
}
