using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TimeSpan = Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate.TimeSpan;

namespace Blog.Services.Blogging.API.Infrastructure;

public static class BloggingDbContextSeed
{
    public async static Task SeedAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BloggingDbContext>()
            .UseNpgsql(connectionString, opt =>
            {
                opt.UseNodaTime();
            })
            .UseSnakeCaseNamingConvention()
            .Options;


        await using (var context = new BloggingDbContext(options))
        {
            context.Database.Migrate();

            var random = new Random();
            for (int i = 100; i < 1000; ++i)
            {
                string userRole = (i % 4) switch
                {
                    0 => UserRole.Administrator.Name,
                    1 => UserRole.Blogger.Name,
                    2 => UserRole.Blogger.Name,
                    3 => UserRole.Administrator.Name
                };

                var transCount = random.Next(1, 3);

                var transLang = random.Next(0, 2);

                IList<PostTranslationBase> translations = new List<PostTranslationBase>();

                for (int j = 0; j < transCount; ++j)
                {
                    if (j >= 1)
                    {
                        int newRandom = transLang;

                        while (transLang == newRandom)
                            newRandom = random.Next(0, 2);

                        transLang = newRandom;
                    }

                    IEnumerable<Tag> tags = (i % 10) switch
                    {
                        0 => new[] { new Tag("post" + i.ToString(), Language.FromValue(transLang)), new Tag("blog" + i.ToString(), Language.FromValue(transLang)), new Tag("elo" + i.ToString(), Language.FromValue(transLang)) },
                        1 => new[] { new Tag("you only yolo once" + i.ToString(), Language.FromValue(transLang)) },
                        2 => new[] { new Tag("tag1" + i.ToString(), Language.FromValue(transLang)), new Tag("tag2" + i.ToString(), Language.FromValue(transLang)) },
                        3 => new[] { new Tag("123" + i.ToString(), Language.FromValue(transLang)) },
                        4 => new[] { new Tag("wpis" + i.ToString(), Language.FromValue(transLang)), new Tag("fsddsfg" + i.ToString(), Language.FromValue(transLang)), new Tag(" rRReree     " + i.ToString(), Language.FromValue(transLang)) },
                        5 => new[] { new Tag(" LoL " + i.ToString(), Language.FromValue(transLang)) },
                        6 => new[] { new Tag("1" + i.ToString(), Language.FromValue(transLang)), new Tag("2" + i.ToString(), Language.FromValue(transLang)) },
                        7 => new[] { new Tag("3" + i.ToString(), Language.FromValue(transLang)), new Tag("4" + i.ToString(), Language.FromValue(transLang)) },
                        8 => new[] { new Tag("5654" + i.ToString(), Language.FromValue(transLang)) },
                        9 => new[] { new Tag("4444444" + i.ToString(), Language.FromValue(transLang)) }
                    };

                    PostTranslationBase translation = (i % 4) switch
                    {
                        0 => new RecipePostTranslation(
                        Language.FromValue(transLang),
                        "recipe " + i.ToString() + " title",
                        "recipe  " + i.ToString() + " content",
                        "recipe " + i.ToString() + "decription this is the psot hwerwe we will talke about some fnie stastsdfa",
                        tags,
                        "dish name " + i.ToString(),
                        "cuisine " + i.ToString(),
                        new[] { "ingredient 1 for recipe " + i.ToString(), "ingredient 2 for recipe " + i.ToString(), "ingredient 3 for recipe " + i.ToString() }
                    ),
                        1 => new RestaurantReviewPostTranslation(
                        new Address("country " + i.ToString(), "52-234", "city " + i.ToString(), "street " + i.ToString()),
                        new[] { "cuisine 1 for restaurantreview " + i.ToString(), "cuisine 2 for restaurantreview " + i.ToString() },
                        Language.FromValue(transLang),
                        "restaurantreview " + i.ToString() + " title",
                        "restaurantreview  " + i.ToString() + " content",
                        "restaurantreview " + i.ToString() + "decription this is the psot hwerwe we will talke about some fnie stastsdfa",
                        tags
                    ),
                        2 => new ProductReviewPostTranslation(
                        Language.FromValue(transLang),
                        "productreview " + i.ToString() + " title",
                        "productreview  " + i.ToString() + " content",
                        "productreview " + i.ToString() + "decription this is the psot hwerwe we will talke about some fnie stastsdfa",
                        tags
                    ),
                        3 => new LifestylePostTranslation(
                        Language.FromValue(transLang),
                        "productreview " + i.ToString() + " title",
                        "productreview  " + i.ToString() + " content",
                        "productreview " + i.ToString() + "decription this is the psot hwerwe we will talke about some fnie stastsdfa",
                        tags
                    )
                    };
                    translations.Add(translation);
                }

                PostBase post = null;

                switch (i % 4)
                {
                    case 0:
                        post = new RecipePost(
                            new User(new UserId(Guid.NewGuid()), "Adrian " + i.ToString(), userRole),
                            translations.Cast<RecipePostTranslation>(),
                            Meal.FromValue(i % 7),
                            RecipeDifficulty.FromValue(i % 3),
                            new RecipeTime(
                                new TimeSpan((i % 2).Hours(), ((i % 60) + 1).Minutes()),
                                new TimeSpan((i % 3).Hours(), (i % 34).Minutes())
                            ),
                            new Servings(i % 6),
                            FoodComposition.FromValue(i % 6),
                            new[] { Taste.FromValue(random.Next(0, 2)), Taste.FromValue(random.Next(2, 5)) },
                            new[] { PreparationMethod.FromValue(random.Next(0, 2)), PreparationMethod.FromValue(random.Next(3, 5)) },
                            "www.wp.pl",
                            "www header pl");
                        break;

                    case 1:
                        post = new RestaurantReviewPost(
                            new User(new UserId(Guid.NewGuid()), "Adrian " + i.ToString(), userRole),
                            translations.Cast<RestaurantReviewPostTranslation>(),
                            new Restaurant("restaurant for post " + i.ToString()),
                            new Rating(random.Next(1, 6)),
                            "header kropka pl");
                        break;

                    case 2:
                        post = new ProductReviewPost(
                            new User(new UserId(Guid.NewGuid()), "Adrian " + i.ToString(), userRole),
                            translations.Cast<ProductReviewPostTranslation>(),
                            new Product("product for post " + i.ToString()),
                            new Rating(random.Next(1, 6)),
                            "header kropka pl");
                        break;

                    case 3:
                        post = new LifestylePost(
                            new User(new UserId(Guid.NewGuid()), "Adrian " + i.ToString(), userRole),
                            translations.Cast<LifestylePostTranslation>(),
                            "header kropka pl");
                        break;
                };

                context.Add(post);
            }
            await context.SaveChangesAsync();
        };
    }
}
