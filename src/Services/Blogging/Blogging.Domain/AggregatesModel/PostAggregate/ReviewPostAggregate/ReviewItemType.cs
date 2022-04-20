using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class ReviewItemType : Enumeration
{
    private ReviewItemType(int value, string name) : base(value, name) { }

    public static readonly ReviewItemType Product = new(0, nameof(Product).ToLowerInvariant());
    public static readonly ReviewItemType Restaurant = new(1, nameof(Restaurant).ToLowerInvariant());

    public static IEnumerable<ReviewItemType> List() => new[] {
        Product,
        Restaurant };

    public static ReviewItemType FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var itemType = List()
            .SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (itemType is null)
            throw new BloggingDomainException($"Possible {nameof(ReviewItemType)} names: {string.Join(", ", List().Select(t => t.Name))}. Provided name: {name}");

        return itemType;
    }

    public static ReviewItemType FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var itemType = List()
            .SingleOrDefault(t => t.Value == value);

        if (itemType is null)
            throw new BloggingDomainException($"Possible {nameof(ReviewItemType)} values: {string.Join(',', List().Select(t => t.Value))}. Provided value: {value}");

        return itemType;
    }
}
