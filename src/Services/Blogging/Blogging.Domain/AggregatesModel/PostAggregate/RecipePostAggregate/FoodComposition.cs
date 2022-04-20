using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class FoodComposition : Enumeration
{
    private FoodComposition(int value, string name) : base(value, name) { }
    public static readonly FoodComposition Frutarian = new(0, nameof(Frutarian).ToLowerInvariant());
    public static readonly FoodComposition Vegan = new(1, nameof(Vegan).ToLowerInvariant());
    public static readonly FoodComposition Vegetarian = new(2, nameof(Vegetarian).ToLowerInvariant());
    public static readonly FoodComposition Fish = new(3, nameof(Fish).ToLowerInvariant());
    public static readonly FoodComposition Meat = new(4, nameof(Meat).ToLowerInvariant());
    public static readonly FoodComposition Seafood = new(5, nameof(Seafood).ToLowerInvariant());
    public static IEnumerable<FoodComposition> List() => new[] {
        Frutarian,
        Vegan,
        Vegetarian,
        Fish,
        Meat,
        Seafood };

    public static FoodComposition FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var composition = List()
            .SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (composition is null)
            throw new BloggingDomainException($"Possible {nameof(FoodComposition)} names: {string.Join(", ", List().Select(c => c.Name))}. Provided name: {name}");

        return composition;
    }

    public static FoodComposition FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var composition = List()
            .SingleOrDefault(c => c.Value == value);

        if (composition is null)
            throw new BloggingDomainException($"Possible {nameof(FoodComposition)} values: {string.Join(',', List().Select(c => c.Value))}. Provided value: {value}");

        return composition;
    }
}
