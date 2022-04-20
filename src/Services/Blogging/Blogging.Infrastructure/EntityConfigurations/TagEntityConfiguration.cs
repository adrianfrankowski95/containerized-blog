using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class TagEntityConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags", BloggingDbContext.DefaultSchema);

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasConversion(x => x.Value, x => new TagId(x));

        builder
            .HasMany<PostTranslationBase>("PostTranslations")
            .WithMany(x => x.Tags)
            .UsingEntity<Dictionary<string, object>>("post_translations_tags",
                j => j.HasOne<PostTranslationBase>().WithMany().HasForeignKey("post_translation_id").OnDelete(DeleteBehavior.SetNull),
                j => j.HasOne<Tag>().WithMany().HasForeignKey("tag_id").OnDelete(DeleteBehavior.SetNull),
                j => j.ToTable("post_translations_tags", BloggingDbContext.DefaultSchema));

        builder
            .Property(x => x.Language)
            .HasConversion(x => x.Name, x => Language.FromName(x))
            .HasColumnType("varchar(50)");

        builder
             .Property(x => x.Value)
             .HasColumnType("varchar(50)");

        builder
            .HasIndex(x => x.Value)
            .HasMethod("btree");

        builder
            .HasIndex(x => new { x.Language, x.Value })
            .IsUnique();

        builder.UseXminAsConcurrencyToken();
    }
}
