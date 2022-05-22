using Blog.Services.Authorization.API.Models;
;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Blog.Services.Authorization.API.Infrastructure.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .ToTable("users", AuthDbContext.DefaultSchema);

        builder
            .Property(x => x.Language)
            .HasConversion<string>()
            .HasMaxLength(256);

        builder
            .Property(x => x.PasswordResetCode)
            .HasMaxLength(256);
    }
}
