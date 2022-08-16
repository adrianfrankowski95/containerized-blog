using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public abstract class ReviewPostTranslationBase : PostTranslationBase
{

    //ef core
    protected ReviewPostTranslationBase() { }

    public ReviewPostTranslationBase(
        Language language,
        string title,
        string content,
        string description,
        IEnumerable<Tag> tags)
        : base(language, title, content, description, tags) { }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        return base.GetEqualityCheckAttributes();
    }

    public override void Validate()
    {
        base.Validate();
    }
}
