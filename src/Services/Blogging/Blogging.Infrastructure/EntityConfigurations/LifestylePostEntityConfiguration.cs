using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class LifestylePostEntityConfiguration : IEntityTypeConfiguration<LifestylePost>
{
    public void Configure(EntityTypeBuilder<LifestylePost> builder)
    {
        builder
            .HasBaseType<PostBase>();
    }
}
