using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public abstract class PostTranslationBase : ValueObject<PostTranslationBase>, ITranslated, IValidatable
{
    public PostId PostId { get; private set; }
    public Language Language { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string Description { get; private set; }

    private readonly HashSet<Tag> _tags;
    public IReadOnlySet<Tag> Tags => _tags;



    //ef core
    protected PostTranslationBase()
    {

    }

    protected PostTranslationBase(
        Language language,
        string title,
        string content,
        string description,
        IEnumerable<Tag> tags)
    {
        if (language is null)
            throw new BloggingDomainException($"{nameof(Language)} cannot be null");

        if (tags.Any(x => !x.Language.Equals(Language)))
            throw new BloggingDomainException($"{nameof(Tags)} must have the same language as translation");

        Language = language;
        Title = title;
        Content = content;
        Description = description;

        _tags = new(tags);
    }

    public PostTranslationBase AssignPost(PostId postId)
    {
        if (postId is null)
            throw new BloggingDomainException($"{nameof(PostId)} cannot be null");

        if (postId.IsEmpty())
            throw new BloggingDomainException($"{nameof(PostId)} cannot be empty");

        var newTranslation = Copy();
        newTranslation.PostId = postId;

        return newTranslation;
    }

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return Title;
        yield return Description;
        yield return Content;
        yield return Tags;
        yield return Language;
    }

    public virtual void Validate()
    {
        if (PostId is null)
            throw new BloggingDomainException($"{nameof(PostId)} cannot be null");

        if (PostId.IsEmpty())
            throw new BloggingDomainException($"{nameof(PostId)} cannot be empty");

        if (Language is null)
            throw new BloggingDomainException($"{nameof(Language)} cannot be null");

        if (string.IsNullOrWhiteSpace(Title))
            throw new BloggingDomainException($"{nameof(Title)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(Content))
            throw new BloggingDomainException($"{nameof(Content)} cannot be null or empty");

        if (string.IsNullOrWhiteSpace(Description))
            throw new BloggingDomainException($"{nameof(Description)} cannot be null or empty");

        if (Tags is null)
            throw new BloggingDomainException($"{nameof(Tags)} cannot be null");

        if (Tags.Count == 0)
            throw new BloggingDomainException($"{nameof(Tags)} cannot be empty");

        if (Tags.Any(x => !x.Language.Equals(Language)))
            throw new BloggingDomainException($"{nameof(Tags)} contain incorrect {nameof(Language)}");

    }
}
