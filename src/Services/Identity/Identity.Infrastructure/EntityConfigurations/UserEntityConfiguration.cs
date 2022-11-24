using System.Text.Json;
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
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General);
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
            .Property(x => x.Username)
            .HasConversion(v => v.Value, v => new Username(v))
            .HasMaxLength(MaxUsernameLength)
            .IsRequired();

        builder.HasIndex(x => x.Username).HasMethod("hash").IsUnique();

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.EmailAddress)
            .HasMaxLength(MaxEmailAddressLength)
            .HasConversion(
                v => JsonSerializer.Serialize<EmailAddress>(v, serializerOptions),
                v => JsonSerializer.Deserialize<EmailAddress>(v, serializerOptions))
            .IsRequired();

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.EmailAddress)
            .HasMaxLength(MaxEmailAddressLength)
            .HasConversion(
                v => JsonSerializer.Serialize<EmailAddress>(v, serializerOptions),
                v => JsonSerializer.Deserialize<EmailAddress>(v, serializerOptions))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(x => x.EmailAddress).HasMethod("hash").IsUnique();

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.EmailConfirmationCode)
            .HasConversion(
                v => JsonSerializer.Serialize<EmailConfirmationCode>(v, serializerOptions),
                v => JsonSerializer.Deserialize<EmailConfirmationCode>(v, serializerOptions))
            .HasColumnType("jsonb")
            .HasDefaultValue(EmailConfirmationCode.Empty)
            .IsRequired(false);

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.FullName)
            .HasConversion(
                v => v.ToString(),
                v => FullName.FromString(v))
            .HasMaxLength(MaxFirstNameLength + MaxLastNameLength + 1)
            .IsRequired(true);

        builder.Property(x => x.Gender).HasConversion(v => v.Name, v => Gender.FromName(v)).HasMaxLength(30);

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.FailedLoginAttempts)
            .HasConversion(
                v => JsonSerializer.Serialize<FailedLoginAttempts>(v, serializerOptions),
                v => JsonSerializer.Deserialize<FailedLoginAttempts>(v, serializerOptions))
            .HasColumnType("jsonb")
            .HasDefaultValue(FailedLoginAttempts.None)
            .IsRequired(true);

        builder
            .Property(x => x.PasswordHash)
            .HasConversion<string>()
            .IsRequired(false);

        // Structs cannot be mapped as owned types in EF 7
        builder
            .Property(x => x.SecurityStamp)
            .HasConversion(
                v => JsonSerializer.Serialize<SecurityStamp>(v, serializerOptions),
                v => JsonSerializer.Deserialize<SecurityStamp>(v, serializerOptions))
            .HasColumnType("jsonb")
            .IsRequired();

        //builder.(x => x.EmailAddress);

        // builder
        //     .OwnsOne(x => x.EmailAddress, u =>
        //     {
        //         u.Property("Value")
        //             .HasColumnName("email_address")
        //             .HasMaxLength(MaxEmailAddressLength)
        //             .IsRequired();

        //         u.HasIndex("_value").HasMethod("hash").IsUnique();

        //         u.Property(x => x.IsConfirmed).HasColumnName("email_address_confirmed").HasDefaultValue(false).IsRequired();
        //     });
        //     // .Navigation(x => x.EmailAddress)
        //     // .IsRequired();

        // builder
        //     .OwnsOne(x => x.EmailConfirmationCode, x =>
        //     {
        //         x.Property<Guid>("_value").HasColumnName("email_confirmation_code").IsRequired(false);
        //         x.Property(x => x.IssuedAt).HasColumnName("email_confirmation_code_issued_at").IsRequired(false);
        //         x.Property(x => x.ValidUntil).HasColumnName("email_confirmation_code_valid_until").IsRequired(false);
        //         x.WithOwner();
        //     })
        //     .Navigation(x => x.EmailConfirmationCode)
        //     .IsRequired(false);

        // builder
        //     .OwnsOne(x => x.FailedLoginAttemptsCount, x =>
        //     {
        //         x.Property<NonNegativeInt>("_count").HasDefaultValue(0).HasColumnName("failed_login_attempts");
        //         x.Property(x => x.LastFailAt).HasColumnName("last_failed_login_attempt_at");
        //         x.Property(x => x.ValidUntil).HasColumnName("failed_login_attempts_valid_until");
        //         x.WithOwner();
        //     })
        //     .Navigation(x => x.FailedLoginAttemptsCount)
        //     .IsRequired();
        // builder
        //     .OwnsOne(x => x.FullName, x =>
        //     {
        //         x.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(MaxFirstNameLength);
        //         x.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(MaxLastNameLength);
        //         x.WithOwner();
        //     });
        // 
        // builder
        //     .OwnsOne(x => x.PasswordHash, x =>
        //     {
        //         x.Property<NonEmptyString>("_value").HasColumnName("password_hash").IsRequired();
        //         x.WithOwner();
        //     })
        //     .Navigation(x => x.PasswordHash)
        //     .IsRequired();

        // builder
        //     .OwnsOne(x => x.Username, x =>
        //     {
        //         x.Property<NonEmptyString>("_value").HasColumnName("username").HasMaxLength(MaxUsernameLength).IsRequired();
        //         x.HasIndex("_value").HasMethod("hash").IsUnique();
        //         x.WithOwner();
        //     })
        //     .Navigation(x => x.Username)
        //     .IsRequired();

        // builder
        //     .OwnsOne(x => x.SecurityStamp, x =>
        //     {
        //         x.Property<Guid>("_value").HasColumnName("security_stamp").IsRequired();
        //         x.Property(x => x.IssuedAt).HasColumnName("security_stamp_issued_at").IsRequired();
        //         x.WithOwner();
        //     })
        //     .Navigation(x => x.SecurityStamp)
        //     .IsRequired();

        builder.Property<byte[]>("row_version").IsRowVersion();
    }
}
