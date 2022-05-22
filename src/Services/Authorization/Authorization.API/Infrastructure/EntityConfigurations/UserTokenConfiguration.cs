using Blog.Services.Authorization.API.Models;
;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Authorization.API.Infrastructure.EntityConfigurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder
            .ToTable("user_tokens", AuthDbContext.DefaultSchema);
    }
}
