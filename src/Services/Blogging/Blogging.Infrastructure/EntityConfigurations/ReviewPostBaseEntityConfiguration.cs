using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class ReviewPostBaseEntityConfiguration : IEntityTypeConfiguration<ReviewPostBase>
{
    public void Configure(EntityTypeBuilder<ReviewPostBase> builder)
    {
        builder
            .HasBaseType<PostBase>();

        builder
           .Property(x => x.Rating)
           .HasConversion(x => x.Value, x => new Rating(x))
           .HasColumnName("review_rating")
           .HasColumnType("integer");
    }
}
