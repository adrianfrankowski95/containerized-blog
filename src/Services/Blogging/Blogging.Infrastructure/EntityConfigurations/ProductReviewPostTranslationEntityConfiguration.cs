using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class ProductReviewPostTranslationEntityConfiguration : IEntityTypeConfiguration<ProductReviewPostTranslation>
{
    public void Configure(EntityTypeBuilder<ProductReviewPostTranslation> builder)
    {
        builder
            .HasBaseType<ReviewPostTranslationBase>();

    }
}
