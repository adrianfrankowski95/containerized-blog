using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.API.Infrastructure.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", IdentityDbContext.DefaultSchema);

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Username)
            .HasColumnType("varchar(32)")
            .IsRequired();

        builder
            .HasIndex(x => x.Username)
            .HasMethod("hash")
            .IsUnique();

        builder
            .Property(x => x.Email)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .HasIndex(x => x.Email)
            .HasMethod("hash")
            .IsUnique();

        builder
            .Property(x => x.Role)
            .HasColumnType("varchar(25)")
            .HasConversion(
                x => x.ToString().ToLowerInvariant(),
                x => Enum.Parse<UserRole>(x));

        builder
            .Property(x => x.Language)
            .HasColumnType("varchar(50)")
            .HasConversion(
                x => x.ToString().ToLowerInvariant(),
                x => Enum.Parse<Language>(x));

        builder
            .Property(x => x.PasswordHash)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.PasswordResetCode)
            .HasColumnType("varchar(20)")
            .IsRequired(false);

        builder
            .Ignore(x => x.LockExists)
            .Ignore(x => x.IsCurrentlyLocked)
            .Ignore(x => x.SuspensionExists)
            .Ignore(x => x.IsCurrentlySuspended)
            .Ignore(x => x.IsResettingPassword);


        builder.UseXminAsConcurrencyToken();
    }
}
