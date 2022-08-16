using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class NutritionalValue : ValueObject<NutritionalValue>
{
    public static readonly Nutrient Carbohydrate = new Nutrient(0, nameof(Carbohydrate).ToLowerInvariant(), NutrientLevel.Undefined);
    public static readonly Nutrient Protein = new Nutrient(1, nameof(Protein).ToLowerInvariant(), NutrientLevel.Undefined);
    public static readonly Nutrient Sugar = new Nutrient(2, nameof(Sugar).ToLowerInvariant(), NutrientLevel.Undefined);
    public static readonly Nutrient Fat = new Nutrient(3, nameof(Fat).ToLowerInvariant(), NutrientLevel.Undefined);
    public static readonly Nutrient Fiber = new Nutrient(4, nameof(Fiber).ToLowerInvariant(), NutrientLevel.Undefined);
    public static readonly Nutrient Salt = new Nutrient(5, nameof(Salt).ToLowerInvariant(), NutrientLevel.Undefined);
    public static IEnumerable<Nutrient> List() => new[] {
        Carbohydrate,
        Protein,
        Sugar,
        Fat,
        Fiber,
        Salt};

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Carbohydrate;
        yield return Protein;
        yield return Sugar;
        yield return Fat;
        yield return Fiber;
        yield return Salt;
    }

    public static Nutrient FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(name)} cannot be null or empty");

        var nutrient = List()
            .SingleOrDefault(n => string.Equals(n.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (nutrient is null)
            throw new BloggingDomainException($"Possible {nameof(Nutrient)} names: {string.Join(',', List().Select(d => d.Name))}, provided name: {name}");

        return nutrient;
    }

    public static Nutrient FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Nutrient.Value)} cannot be null");

        var nutrient = List()
            .SingleOrDefault(n => n.Value == value);

        if (nutrient is null)
            throw new BloggingDomainException($"Possible {nameof(Nutrient.Name)} values: {string.Join(',', List().Select(d => d.Value))}. Provided value: {value}");

        return nutrient;
    }
}
