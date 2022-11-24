using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class PostBaseEntityConfiguration : IEntityTypeConfiguration<PostBase>
{
    public void Configure(EntityTypeBuilder<PostBase> builder)
    {
        builder
            .ToTable("posts", BloggingDbContext.DefaultSchema);

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id).HasConversion(x => x.Value, x => new PostId(x));

        builder
            .Property(x => x.Type)
            .HasConversion(x => x.Name, x => PostType.FromName(x))
            .HasColumnType("varchar(50)")
            .HasColumnName("type");

        builder
            .HasDiscriminator(x => x.Type)
            .HasValue<RecipePost>(PostType.Recipe)
            .HasValue<RestaurantReviewPost>(PostType.RestaurantReview)
            .HasValue<ProductReviewPost>(PostType.ProductReview)
            .HasValue<LifestylePost>(PostType.Lifestyle);

        builder
            .HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder
            .Navigation(x => x.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .OwnsOne(x => x.Author, a =>
            {
                a.Property(x => x.Id)
                    .HasConversion(x => x.Value, x => new UserId(x))
                    .HasColumnName("author_id");

                a.HasIndex(x => x.Id)
                    .HasMethod("hash");

                a.Property(x => x.Name)
                    .HasColumnName("author_name")
                    .HasColumnType("varchar(32)");

                a.Ignore(x => x.Role);

                a.Property<byte[]>("row_version")
                    .HasColumnName("row_version")
                    .IsRowVersion();

                a.WithOwner();
            });

        builder
            .OwnsOne(x => x.Editor, e =>
            {
                e.Property(x => x.Id)
                    .HasConversion(x => x.Value, x => new UserId(x))
                    .HasColumnName("editor_id");

                e.Property(x => x.Name)
                    .HasColumnName("editor_name")
                    .HasColumnType("varchar(32)");

                e.Ignore(x => x.Role);

                e.Property<byte[]>("row_version")
                    .HasColumnName("row_version")
                    .IsRowVersion();

                e.WithOwner();
            });

        builder
            .Property(x => x.Category)
            .HasConversion(x => x.Name, x => PostCategory.FromName(x))
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Status)
            .HasConversion(x => x.Name, x => PostStatus.FromName(x))
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Likes)
            .HasColumnName("likes_count")
            .HasConversion(x => x.Count, x => new Likes(x))
            .HasColumnType("integer")
            .HasDefaultValue(new Likes());

        builder
            .HasIndex(x => x.Likes)
            .HasMethod("btree");

        builder
            .Property(x => x.Views)
            .HasColumnName("views_count")
            .HasConversion(x => x.Count, x => new Views(x))
            .HasColumnType("integer")
            .HasDefaultValue(new Views());

        builder
            .HasIndex(x => x.Views)
            .HasMethod("btree");

        builder
            .Property(x => x.Comments)
            .HasColumnName("comments_count")
            .HasConversion(x => x.Count, x => new Comments(x))
            .HasColumnType("integer")
            .HasDefaultValue(new Comments());

        builder
            .Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder
            .Property(x => x.HeaderImgUrl)
            .HasColumnType("varchar(500)");

        builder
            .HasIndex(x => x.Category)
            .IsDescending(false)
            .HasMethod("hash");

        builder
            .HasIndex(x => new { x.Status, x.CreatedAt })
            .IsDescending(true, true)
            .HasMethod("btree");

        builder
            .HasIndex(x => x.CreatedAt)
            .IsDescending(true)
            .HasMethod("btree");

        builder
            .Property<byte[]>("row_version")
            .HasColumnName("row_version")
            .IsRowVersion();
    }
}
