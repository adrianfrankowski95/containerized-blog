using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.API.Infrastructure.EntityConfigurations;

public class UserEntityConfiguration<TUser> : IEntityTypeConfiguration<TUser> where TUser : User
{
    public void Configure(EntityTypeBuilder<TUser> builder)
    {
        builder.ToTable("users", IdentityConstants.DefaultDbSchema);

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
            .Property(x => x.Name)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .Property(x => x.LastName)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder
            .Ignore(x => x.FullName);

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
                x => x.Name,
                x => Role.FromName(x));

        builder
            .HasOne(x => x.Role)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(x => x.PasswordHash)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.PasswordResetCode)
            .HasColumnType("varchar(20)")
            .IsRequired(false);

        builder
            .Property(x => x.PasswordResetCodeIssuedAt)
            .IsRequired(false);

        builder
            .Property(x => x.EmailConfirmationCode)
            .IsRequired(false);

        builder
            .Property(x => x.EmailConfirmationCodeIssuedAt)
            .IsRequired(false);

        builder.UseXminAsConcurrencyToken();
    }
}
