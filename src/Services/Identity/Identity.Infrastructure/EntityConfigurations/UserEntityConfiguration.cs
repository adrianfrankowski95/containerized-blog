using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    private const int MaxUsernameLength = 20;
    private const int MaxFirstNameLength = 100;
    private const int MaxLastNameLength = 100;
    private const int MaxEmailAddressLength = 50;
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .ToTable("users", IdentityDbContext.DefaultSchema);

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasConversion(v => v.Value, v => UserId.FromGuid(v));

        builder
            .OwnsOne(x => x.EmailAddress, x =>
            {
                x.Property<NonEmptyString>("_value").HasColumnName("email_address").HasMaxLength(MaxEmailAddressLength).IsRequired();
                x.HasIndex("_value").HasMethod("hash").IsUnique();
                x.Property(x => x.IsConfirmed).HasColumnName("email_address_confirmed").HasDefaultValue(false).IsRequired();
                x.WithOwner();
            })
            .Navigation(x => x.EmailAddress)
            .IsRequired();

        builder
            .OwnsOne(x => x.EmailConfirmationCode, x =>
            {
                x.Property<Guid>("_value").HasColumnName("email_confirmation_code").IsRequired(false);
                x.Property(x => x.IssuedAt).HasColumnName("email_confirmation_code_issued_at").IsRequired(false);
                x.Property(x => x.ValidUntil).HasColumnName("email_confirmation_code_valid_until").IsRequired(false);
                x.WithOwner();
            })
            .Navigation(x => x.EmailConfirmationCode)
            .IsRequired(false);

        builder
            .OwnsOne(x => x.FailedLoginAttemptsCount, x =>
            {
                x.Property<NonNegativeInt>("_count").HasDefaultValue(0).HasColumnName("failed_login_attempts");
                x.Property(x => x.LastFailAt).HasColumnName("last_failed_login_attempt_at");
                x.Property(x => x.ValidUntil).HasColumnName("failed_login_attempts_valid_until");
                x.WithOwner();
            })
            .Navigation(x => x.FailedLoginAttemptsCount)
            .IsRequired();

        builder
            .OwnsOne(x => x.FullName, x =>
            {
                x.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(MaxFirstNameLength);
                x.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(MaxLastNameLength);
                x.WithOwner();
            });

        builder.Property(x => x.Gender).HasConversion(v => v.Name, v => Gender.FromName(v)).HasMaxLength(30);

        builder
            .OwnsOne(x => x.PasswordHash, x =>
            {
                x.Property<NonEmptyString>("_value").HasColumnName("password_hash").IsRequired();
                x.WithOwner();
            })
            .Navigation(x => x.PasswordHash)
            .IsRequired();

        builder
            .OwnsOne(x => x.Username, x =>
            {
                x.Property<NonEmptyString>("_value").HasColumnName("username").HasMaxLength(MaxUsernameLength).IsRequired();
                x.HasIndex("_value").HasMethod("hash").IsUnique();
                x.WithOwner();
            })
            .Navigation(x => x.Username)
            .IsRequired();

        builder
            .OwnsOne(x => x.SecurityStamp, x =>
            {
                x.Property<Guid>("_value").HasColumnName("security_stamp").IsRequired();
                x.Property(x => x.IssuedAt).HasColumnName("security_stamp_issued_at").IsRequired();
                x.WithOwner();
            })
            .Navigation(x => x.SecurityStamp)
            .IsRequired();

        builder.Property("row_version").IsRowVersion();
    }
}
