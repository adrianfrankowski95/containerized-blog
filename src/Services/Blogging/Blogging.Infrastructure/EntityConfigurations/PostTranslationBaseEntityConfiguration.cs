using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class PostTranslationBaseEntityConfiguration : IEntityTypeConfiguration<PostTranslationBase>
{
    public void Configure(EntityTypeBuilder<PostTranslationBase> builder)
    {
        builder.ToTable("post_translations", BloggingDbContext.DefaultSchema);

        builder
            .Property<Guid>("Id");

        builder
            .HasKey("Id");

        builder
            .HasDiscriminator<PostType>("translation_type")
            .HasValue<RecipePostTranslation>(PostType.Recipe)
            .HasValue<RestaurantReviewPostTranslation>(PostType.RestaurantReview)
            .HasValue<ProductReviewPostTranslation>(PostType.ProductReview)
            .HasValue<LifestylePostTranslation>(PostType.Lifestyle);

        builder
            .Property<PostType>("translation_type")
            .HasConversion(x => x.Name, x => PostType.FromName(x))
            .HasColumnType("varchar(50)");

        builder
            .HasMany(x => x.Tags)
            .WithMany("PostTranslations");

        builder
            .Navigation(x => x.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder
            .Property(x => x.Language)
            .HasConversion(x => x.Name, x => Language.FromName(x))
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(60)");

        builder
            .Property(x => x.Description)
            .HasColumnType("varchar(200)");

        builder
            .Property(x => x.Content)
            .HasColumnType("text");


        foreach (var language in Language.List())
            builder
                .HasIndex(x => new { x.Title, x.Description, x.Content }) //tags
                .HasDatabaseName("ix_post_translations_base_" + language.Name)
                .IsTsVectorExpressionIndex(language.Name);

        builder
            .HasIndex(x => new { x.PostId, x.Language })
            .HasMethod("btree")
            .IsUnique();

        builder.UseXminAsConcurrencyToken();
    }
}
