using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;
public abstract class ReviewItem : ValueObject<ReviewItem>, IValidatable
{
    public string Name { get; }
    public string WebsiteUrl { get; }

    //ef core
    protected ReviewItem() { }

    protected ReviewItem(string name, string websiteUrl)
    {
        Name = name;
        WebsiteUrl = websiteUrl;
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Name;
        yield return WebsiteUrl;
    }

    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");
    }
}
