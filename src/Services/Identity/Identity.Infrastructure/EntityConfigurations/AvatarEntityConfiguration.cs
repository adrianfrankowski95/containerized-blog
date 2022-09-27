using Blog.Services.Identity.Infrastructure.Avatar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Identity.Infrastructure.EntityConfigurations;

public class AvatarEntityConfiguration : IEntityTypeConfiguration<AvatarModel>
{
    public void Configure(EntityTypeBuilder<AvatarModel> builder)
    {
        builder.ToTable("avatars", IdentityDbContext.DefaultSchema);

        builder.HasKey(x => x.UserId);

        // To make sure that byte arrays are compared referentially and not by iterating each value
        builder
            .Property(x => x.ImageData)
            .IsRequired()
            .Metadata
                .SetValueComparer(
                    new ValueComparer<byte[]>(
                        (first, second) => ReferenceEquals(first, second),
                        data => data.GetHashCode(),
                        data => data));

        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
