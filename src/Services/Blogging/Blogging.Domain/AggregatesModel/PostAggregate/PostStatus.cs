using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;


public sealed class PostStatus : Enumeration
{
    private PostStatus(int value, string name) : base(value, name) { }

    public static readonly PostStatus Draft = new(0, nameof(Draft).ToLowerInvariant());
    public static readonly PostStatus Rejected = new(1, nameof(Rejected).ToLowerInvariant());
    public static readonly PostStatus Deleted = new(2, nameof(Deleted).ToLowerInvariant());
    public static readonly PostStatus Submitted = new(3, nameof(Submitted).ToLowerInvariant());
    public static readonly PostStatus Published = new(4, nameof(Published).ToLowerInvariant());


    public static IEnumerable<PostStatus> List() => new[] {
        Draft,
        Submitted,
        Published,
        Rejected,
        Deleted };

    public static PostStatus FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null or empty");

        var status = List()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (status is null)
            throw new BloggingDomainException($"Possible {nameof(PostStatus)} names: {string.Join(", ", List().Select(s => s.Name))}. Provided name: {name}");

        return status;
    }

    public static PostStatus FromValue(int value)
    {
        if (value == null)
            throw new BloggingDomainException($"{nameof(Value)} cannot be null");

        var status = List()
            .SingleOrDefault(s => s.Value == value);

        if (status is null)
            throw new BloggingDomainException($"Possible {nameof(PostStatus)} values: {string.Join(',', List().Select(s => s.Value))}. Provided value: {value}");

        return status;
    }

}