using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class ReviewPostTranslationBaseEntityConfiguration : IEntityTypeConfiguration<ReviewPostTranslationBase>
{
    public void Configure(EntityTypeBuilder<ReviewPostTranslationBase> builder)
    {
        builder
            .HasBaseType<PostTranslationBase>();
    }
}
