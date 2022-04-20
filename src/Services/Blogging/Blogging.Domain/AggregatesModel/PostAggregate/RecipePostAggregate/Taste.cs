using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class Taste : Enumeration
{
    private Taste(int value, string name) : base(value, name) { }

    public static readonly Taste Undefined = new(-1, nameof(Undefined).ToLowerInvariant());
    public static readonly Taste Savoury = new(0, nameof(Savoury).ToLowerInvariant());
    public static readonly Taste Sweet = new(1, nameof(Sweet).ToLowerInvariant());
    public static readonly Taste Sour = new(2, nameof(Sour).ToLowerInvariant());
    public static readonly Taste Mild = new(3, nameof(Mild).ToLowerInvariant());
    public static readonly Taste Spicy = new(4, nameof(Spicy).ToLowerInvariant());
    public static readonly Taste Hot = new(5, nameof(Hot).ToLowerInvariant());
    public static readonly Taste Bitter = new(6, nameof(Bitter).ToLowerInvariant());
    public static IEnumerable<Taste> List() => new[] {
        Undefined,
        Savoury,
        Sweet,
        Sour,
        Mild,
        Spicy,
        Hot,
        Bitter };

    public static Taste FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var taste = List()
            .SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (taste is null)
            throw new BloggingDomainException($"Possible {nameof(Taste)} names: {string.Join(", ", List().Select(t => t.Name))}. Provided name: {name}");

        return taste;
    }

    public static Taste FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var taste = List()
            .SingleOrDefault(t => t.Value == value);

        if (taste is null)
            throw new BloggingDomainException($"Possible {nameof(Taste)} values: {string.Join(',', List().Select(d => d.Value))}. Provided value: {value}");

        return taste;
    }

}
