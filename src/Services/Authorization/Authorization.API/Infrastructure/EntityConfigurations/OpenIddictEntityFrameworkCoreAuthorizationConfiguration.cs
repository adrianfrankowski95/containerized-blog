using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Auth.API.Infrastructure.EntityConfigurations;

public class OpenIddictEntityFrameworkCoreAuthorizationConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreAuthorization>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreAuthorization> builder)
    {
        builder
            .ToTable("authorizations", AuthDbContext.DefaultSchema);
    }
}
