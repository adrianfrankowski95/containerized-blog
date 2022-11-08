using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public abstract class ReviewPostBase : PostBase
{
    public Rating Rating { get; private set; }

    // EF Core
    protected ReviewPostBase() { }

    protected ReviewPostBase(
        User author,
        PostType type,
        IEnumerable<ReviewPostTranslationBase> translations,
        Rating rating,
        string headerImgUrl)
        : base(author, PostCategory.Review, type, translations, headerImgUrl)
    {
        Rating = rating;
    }

    protected virtual bool UpdateBy(
        User editor,
        IEnumerable<ReviewPostTranslationBase> newTranslations,
        Rating newRating,
        string headerImgUrl)
    {
        bool isChanged = false;

        if (!Rating.Equals(newRating))
        {
            Rating = newRating;
            isChanged = true;
        }

        return base.UpdateBy(editor, newTranslations, headerImgUrl) || isChanged;
    }

    public override void Validate()
    {
        if (Rating is null)
            throw new BloggingDomainException($"{nameof(Rating)} cannot be null");

        Rating.Validate();

        base.Validate();
    }
}
