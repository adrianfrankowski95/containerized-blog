using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class ProductReviewPostEntityConfiguration : IEntityTypeConfiguration<ProductReviewPost>
{
    public void Configure(EntityTypeBuilder<ProductReviewPost> builder)
    {
        builder
            .HasBaseType<ReviewPostBase>();

        builder
            .OwnsOne(x => x.Product, p =>
            {
                p.Property(x => x.Name)
                    .HasColumnName("review_item_name")
                    .HasColumnType("varchar(60)");
                p.WithOwner();

                p.Property(x => x.WebsiteUrl)
                    .HasColumnName("review_item_website_url")
                    .HasColumnType("varchar(500)")
                    .IsRequired(false);

                p.Property("row_version").IsRowVersion();

                p.WithOwner();
            });

    }
}
