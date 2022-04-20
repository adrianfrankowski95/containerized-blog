using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Auth.API.Infrastructure.EntityConfigurations;

public class OpenIddictEntityFrameworkCoreTokenConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreToken>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreToken> builder)
    {
        builder
            .ToTable("tokens", AuthDbContext.DefaultSchema);
    }
}
