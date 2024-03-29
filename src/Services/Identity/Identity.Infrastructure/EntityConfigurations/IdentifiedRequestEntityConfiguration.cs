using Blog.Services.Identity.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class IdentifiedRequestEntityConfiguration : IEntityTypeConfiguration<IdentifiedRequest>
{
    public void Configure(EntityTypeBuilder<IdentifiedRequest> builder)
    {
        builder.ToTable("requests", IdentityDbContext.DefaultSchema);

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Type)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
    }
}
