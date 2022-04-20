using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class LifestylePostTranslationEntityConfiguration : IEntityTypeConfiguration<LifestylePostTranslation>
{
    public void Configure(EntityTypeBuilder<LifestylePostTranslation> builder)
    {
        builder
            .HasBaseType<PostTranslationBase>();

    }
}
