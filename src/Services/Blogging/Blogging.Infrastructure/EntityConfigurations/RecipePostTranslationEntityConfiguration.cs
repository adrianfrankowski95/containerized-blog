using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class RecipePostTranslationEntityConfiguration : IEntityTypeConfiguration<RecipePostTranslation>
{
    public void Configure(EntityTypeBuilder<RecipePostTranslation> builder)
    {
        builder
            .HasBaseType<PostTranslationBase>();

        builder
            .Property(x => x.Cuisine)
            .HasColumnName("recipe_cuisine")
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.DishName)
            .HasColumnName("recipe_dish_name")
            .HasColumnType("varchar(50)");

        //A workaround for keeping encapsulation in a Domain
        //and mapping to an array column type in Postgresql
        builder
            .Ignore(x => x.Ingredients); //property of type IReadOnlyList<string>
        builder
            .Property("_ingredients") //backing field of type List<string>
            .HasColumnName("recipe_ingredients")
            .HasColumnType("text[]");

        foreach (var language in Language.List())
            builder
                .HasIndex(x => new { x.Title, x.Description, x.Content, x.Cuisine, x.DishName })
                .HasDatabaseName("ix_post_translations_recipe_" + language.Name)
                .IsTsVectorExpressionIndex(language.Name);
    }
}
