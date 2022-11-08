using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Meal : Enumeration
{
    private Meal(int value, string name) : base(value, name) { }

    public static readonly Meal Undefined = new(-1, nameof(Undefined).ToLowerInvariant());
    public static readonly Meal Breakfast = new(0, nameof(Breakfast).ToLowerInvariant());
    public static readonly Meal Starter = new(1, nameof(Starter).ToLowerInvariant());
    public static readonly Meal FirstCourse = new(2, nameof(FirstCourse).ToLowerInvariant());
    public static readonly Meal MainCourse = new(3, nameof(MainCourse).ToLowerInvariant());
    public static readonly Meal Snack = new(4, nameof(Snack).ToLowerInvariant());
    public static readonly Meal Dessert = new(5, nameof(Dessert).ToLowerInvariant());
    public static readonly Meal Supper = new(6, nameof(Supper).ToLowerInvariant());


    public static IEnumerable<Meal> List() => new[] {
        Undefined,
        Breakfast,
        Starter,
        FirstCourse,
        MainCourse,
        Snack,
        Dessert,
        Supper };

    public static Meal FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var meal = List()
            .SingleOrDefault(m => string.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (meal is null)
            throw new BloggingDomainException($"Possible {nameof(Meal)} names: {string.Join(", ", List().Select(m => m.Name))}. Provided name: {name}");

        return meal;
    }

    public static Meal FromValue(int value)
    {
        var meal = List()
            .SingleOrDefault(m => m.Value == value);

        if (meal is null)
            throw new BloggingDomainException($"Possible {nameof(Meal)} values: {string.Join(',', List().Select(m => m.Value))}. Provided value: {value}");

        return meal;
    }
}
