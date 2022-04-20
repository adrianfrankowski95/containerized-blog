using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class NutrientLevel : Enumeration
{
    private NutrientLevel(int value, string name) : base(value, name) { }

    public static readonly NutrientLevel Undefined = new(-1, nameof(Undefined).ToLowerInvariant());
    public static readonly NutrientLevel Low = new(0, nameof(Low).ToLowerInvariant());
    public static readonly NutrientLevel Medium = new(1, nameof(Medium).ToLowerInvariant());
    public static readonly NutrientLevel High = new(2, nameof(High).ToLowerInvariant());

    public static IEnumerable<NutrientLevel> List() => new[] {
        Undefined,
        Low,
        Medium,
        High };

    public static NutrientLevel FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var level = List()
            .SingleOrDefault(l => string.Equals(l.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (level is null)
            throw new BloggingDomainException($"Possible {nameof(NutrientLevel)} names: {string.Join(", ", List().Select(l => l.Name))}. Provided name: {name}");

        return level;
    }

    public static NutrientLevel FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var level = List()
            .SingleOrDefault(l => l.Value == value);

        if (level is null)
            throw new BloggingDomainException($"Possible {nameof(NutrientLevel)} values: {string.Join(',', List().Select(m => m.Value))}. Provided value: {value}");

        return level;
    }
}
