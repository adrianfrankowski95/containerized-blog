using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public sealed class PostType : Enumeration
{
    private PostType(int value, string name) : base(value, name) { }

    public static readonly PostType Recipe = new(0, nameof(Recipe).ToLowerInvariant());
    public static readonly PostType Lifestyle = new(1, nameof(Lifestyle).ToLowerInvariant());
    public static readonly PostType ProductReview = new(2, nameof(ProductReview).ToLowerInvariant());
    public static readonly PostType RestaurantReview = new(3, nameof(RestaurantReview).ToLowerInvariant());


    public static IEnumerable<PostType> List() => new[] {
        Recipe,
        Lifestyle,
        ProductReview,
        RestaurantReview };

    public static PostType FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var type = List()
            .SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (type is null)
            throw new BloggingDomainException($"Possible {nameof(PostType)} names: {string.Join(", ", List().Select(c => c.Name))}. Provided name: {name}");

        return type;
    }

    public static PostType FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var type = List()
            .SingleOrDefault(c => c.Value == value);

        if (type is null)
            throw new BloggingDomainException($"Possible {nameof(PostType)} values: {string.Join(',', List().Select(c => c.Value))}. Provided value: {value}");

        return type;
    }
}
