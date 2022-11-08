using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public sealed class PostCategory : Enumeration
{
    private PostCategory(int value, string name) : base(value, name) { }

    public static readonly PostCategory Recipe = new(0, nameof(Recipe).ToLowerInvariant());
    public static readonly PostCategory Lifestyle = new(1, nameof(Lifestyle).ToLowerInvariant());
    public static readonly PostCategory Review = new(2, nameof(Review).ToLowerInvariant());

    public static IEnumerable<PostCategory> List() => new[] {
        Recipe,
        Lifestyle,
        Review, };

    public static PostCategory FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var category = List()
            .SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (category is null)
            throw new BloggingDomainException($"Possible {nameof(PostCategory)} names: {string.Join(", ", List().Select(c => c.Name))}. Provided name: {name}");

        return category;
    }

    public static PostCategory FromValue(int value)
    {
        var category = List()
            .SingleOrDefault(c => c.Value == value);

        if (category is null)
            throw new BloggingDomainException($"Possible {nameof(PostCategory)} values: {string.Join(',', List().Select(c => c.Value))}. Provided value: {value}");

        return category;
    }
}
