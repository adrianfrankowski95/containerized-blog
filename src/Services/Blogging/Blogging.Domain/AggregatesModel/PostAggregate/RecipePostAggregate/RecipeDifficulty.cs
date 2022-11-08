using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.RecipePostAggregate;

public sealed class RecipeDifficulty : Enumeration
{
    private RecipeDifficulty(int value, string name) : base(value, name) { }

    public static readonly RecipeDifficulty Easy = new(0, nameof(Easy).ToLowerInvariant());
    public static readonly RecipeDifficulty Medium = new(1, nameof(Medium).ToLowerInvariant());
    public static readonly RecipeDifficulty Hard = new(2, nameof(Hard).ToLowerInvariant());

    public static IEnumerable<RecipeDifficulty> List() => new[] {
        Easy,
        Medium,
        Hard };

    public static RecipeDifficulty FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var difficulty = List()
            .SingleOrDefault(d => string.Equals(d.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (difficulty is null)
            throw new BloggingDomainException($"Possible {nameof(RecipeDifficulty)} names: {string.Join(", ", List().Select(d => d.Name))}. Provided name: {name}");

        return difficulty;
    }

    public static RecipeDifficulty FromValue(int value)
    {
        var difficulty = List()
            .SingleOrDefault(d => d.Value == value);

        if (difficulty is null)
            throw new BloggingDomainException($"Possible {nameof(RecipeDifficulty)} values: {string.Join(',', List().Select(d => d.Value))}. Provided value: {value}");

        return difficulty;
    }
}
