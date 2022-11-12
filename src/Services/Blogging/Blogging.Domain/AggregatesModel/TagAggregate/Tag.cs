using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.Exceptions;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;

public class Tag : Entity<TagId>, ITranslated, IAggregateRoot
{
    public Language Language { get; private set; }
    public string Value { get; }

    public Tag(string tag, Language language)
    {
        string trimmedTag = tag.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(trimmedTag))
            throw new BloggingDomainException($"{nameof(Tag)} cannot be null or empty");

        if (language is null)
            throw new BloggingDomainException($"{nameof(Language)} cannot be null or empty");

        Id = new TagId();
        Value = trimmedTag;
        Language = language;
    }

    public override int GetHashCode()
    {
        return Language.Value;
    }

    public override bool Equals(object? second)
    {
        if (second is null)
            return false;

        if (second is not Tag tag)
            return false;

        bool equalLanguage = Language.Equals(tag.Language);
        bool equalValue = string.Equals(Value, tag.Value, StringComparison.CurrentCultureIgnoreCase);

        return equalLanguage && equalValue;
    }
}

public class TagId : ValueObject<TagId>
{
    public Guid Value { get; }

    public TagId() => Value = Guid.NewGuid();

    public TagId(Guid tagId) => Value = tagId;

    public bool IsEmpty => Value.Equals(Guid.Empty);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}