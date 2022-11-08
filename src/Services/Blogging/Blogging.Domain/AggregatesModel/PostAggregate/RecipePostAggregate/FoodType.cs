using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class FoodType : Enumeration
{
    private FoodType(int value, string name) : base(value, name) { }

    public static readonly FoodType Undefined = new(-1, nameof(Undefined).ToLowerInvariant());
    public static readonly FoodType SlowFood = new(0, nameof(SlowFood).ToLowerInvariant());
    public static readonly FoodType FastFood = new(1, nameof(FastFood).ToLowerInvariant());
    public static readonly FoodType Soup = new(2, nameof(Soup).ToLowerInvariant());
    public static readonly FoodType Cream = new(3, nameof(Cream).ToLowerInvariant());
    public static readonly FoodType Smoothie = new(4, nameof(Smoothie).ToLowerInvariant());
    public static readonly FoodType Bread = new(5, nameof(Bread).ToLowerInvariant());
    public static readonly FoodType Fit = new(6, nameof(Fit).ToLowerInvariant());
    public static readonly FoodType Cake = new(7, nameof(Cake).ToLowerInvariant());
    public static readonly FoodType Pasta = new(8, nameof(Pasta).ToLowerInvariant());
    public static readonly FoodType Pizza = new(9, nameof(Pizza).ToLowerInvariant());
    public static readonly FoodType Spread = new(10, nameof(Spread).ToLowerInvariant());

    public static IEnumerable<FoodType> List() => new[] {
        Undefined,
        SlowFood,
        FastFood,
        Soup,
        Cream,
        Smoothie,
        Bread,
        Fit,
        Cake,
        Pasta,
        Pizza,
        Spread };

    public static FoodType FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var type = List()
            .SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (type is null)
            throw new BloggingDomainException($"Possible {nameof(FoodType)} names: {string.Join(", ", List().Select(t => t.Name))}. Provided name: {name}");

        return type;
    }

    public static FoodType FromValue(int value)
    {
        var type = List()
            .SingleOrDefault(t => t.Value == value);

        if (type is null)
            throw new BloggingDomainException($"Possible {nameof(FoodType)} values: {string.Join(',', List().Select(t => t.Value))}. Provided value: {value}");

        return type;
    }
}
