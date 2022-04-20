using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Nutrient : Enumeration
{
    public NutrientLevel Level { get; private set; }
    public Nutrient(int value, string name, NutrientLevel level) : base(value, name) { Level = level; }

    public void SetLevel(NutrientLevel level)
    {
        Level = level;
    }
}
