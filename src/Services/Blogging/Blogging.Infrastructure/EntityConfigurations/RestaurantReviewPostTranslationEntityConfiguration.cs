using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Services.Blogging.Infrastructure.EntityConfigurations;

public class RestaurantReviewPostTranslationEntityConfiguration : IEntityTypeConfiguration<RestaurantReviewPostTranslation>
{
    public void Configure(EntityTypeBuilder<RestaurantReviewPostTranslation> builder)
    {
        builder
            .HasBaseType<ReviewPostTranslationBase>();

        builder
            .OwnsOne(x => x.RestaurantAddress, a =>
            {
                a.Property(x => x.Street)
                    .HasColumnName("review_restaurant_street")
                    .HasColumnType("varchar(50)");

                a.Property(x => x.City)
                    .HasColumnName("review_restaurant_city")
                    .HasColumnType("varchar(50)");

                a.Property(x => x.Country)
                    .HasColumnName("review_restaurant_country")
                    .HasColumnType("varchar(50)");

                a.Property(x => x.ZipCode)
                    .HasColumnName("review_restaurant_zipcode")
                    .HasColumnType("varchar(10)");

                a.Property("row_version").IsRowVersion();

                a.WithOwner();
            });

        //A workaround for keeping encapsulation in a Domain
        //and mapping to an array column type in Postgresql
        builder
            .Ignore(x => x.RestaurantCuisines); //property of type IReadOnlyList<string>
        builder
            .Property("_restaurantCuisines") //backing field of type List<string>
            .HasColumnName("review_restaurant_cuisines")
            .HasColumnType("text[]");

        //foreach (var language in Language.List())
        //    builder
        //        .HasIndex(x => new { x.Title, x.Description, x.Content }) //restaurantcuisines restaurantaddress
        //        .IsTsVectorExpressionIndex(language.Name);
    }
}
