using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.TypeMapping;
using TimeSpan = Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate.TimeSpan;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class RecipePostEntityConfiguration : IEntityTypeConfiguration<RecipePost>
{
    public void Configure(EntityTypeBuilder<RecipePost> builder)
    {
        builder
            .HasBaseType<PostBase>();

        builder
            .Property(x => x.FoodComposition)
            .HasConversion(x => x.Name, x => FoodComposition.FromName(x))
            .HasColumnName("recipe_food_composition")
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Difficulty)
            .HasConversion(x => x.Name, x => RecipeDifficulty.FromName(x))
            .HasColumnName("recipe_difficulty")
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Meal)
            .HasConversion(x => x.Name, x => Meal.FromName(x))
            .HasColumnName("recipe_meal")
            .HasColumnType("varchar(50)");

        builder
            .Property(x => x.Servings)
            .HasConversion(x => x.Count, x => new Servings(x))
            .HasColumnName("recipe_servings_count")
            .HasColumnType("integer");

        builder
            .Property(x => x.SongUrl)
            .HasColumnName("recipe_song_url")
            .HasColumnType("varchar(500)");

        builder
            .OwnsOne(x => x.Time, t =>
            {
                t.Property(x => x.PreparationTime)
                    .HasConversion(
                        x => x.ToMinutes().AsInt(),
                        x => TimeSpan.FromMinutes(x.Minutes()))
                    .HasColumnName("recipe_preparation_minutes")
                    .HasColumnType("integer");

                t.Property(x => x.CookingTime)
                    .HasConversion(
                        x => x.ToMinutes().AsInt(),
                        x => TimeSpan.FromMinutes(x.Minutes()))
                    .HasColumnName("recipe_cooking_minutes")
                    .HasColumnType("integer");

                t.Property("row_version").IsRowVersion(); ;

                t.Ignore(x => x.TotalTime);

                t.WithOwner();
            });

        //A workaround for keeping encapsulation in a Domain
        //and mapping to an array column type in Postgresql
        builder
            .Ignore(x => x.Tastes); //property of type IReadOnlyList<Taste>
        builder
            .Property<List<Taste>>("_tastes") //backing field of type List<Taste>
            .HasPostgresArrayConversion(x => x.Name, x => Taste.FromName(x))
            .HasColumnName("recipe_tastes")
            .HasColumnType("text[]");

        //A workaround for keeping encapsulation in a Domain
        //and mapping to an array column type in Postgresql
        builder
            .Ignore(x => x.PreparationMethods); //property of type IReadOnlyList<PreparationMethod>
        builder
            .Property<List<PreparationMethod>>("_preparationMethods") //backing field of type List<PreparationMethod>
            .HasPostgresArrayConversion(x => x.Name, x => PreparationMethod.FromName(x))
            .HasColumnName("recipe_preparation_methods")
            .HasColumnType("text[]");

    }
}
