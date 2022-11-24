using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class RestaurantReviewPostEntityConfiguration : IEntityTypeConfiguration<RestaurantReviewPost>
{
    public void Configure(EntityTypeBuilder<RestaurantReviewPost> builder)
    {
        builder
            .HasBaseType<ReviewPostBase>();

        builder
            .OwnsOne(x => x.Restaurant, r =>
            {
                r.Property(x => x.Name)
                    .HasColumnName("review_item_name")
                    .HasColumnType("varchar(60)");

                r.Property(x => x.WebsiteUrl)
                    .HasColumnName("review_item_website_url")
                    .HasColumnType("varchar(500)")
                    .IsRequired(false);

                r.Property<byte[]>("row_version")
                    .HasColumnName("row_version")
                    .IsRowVersion();

                r.WithOwner();
            });
    }
}
