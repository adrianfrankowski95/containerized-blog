using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;

public class LifestylePost : PostBase
{
    //ef core
    private LifestylePost() { }

    public LifestylePost(
        User author,
        IEnumerable<LifestylePostTranslation> translations,
        string headerImgUrl)
        : base(author, translations, headerImgUrl)
    {
        Type = PostType.Lifestyle;
        Category = PostCategory.Lifestyle;
    }

    public bool UpdateBy(User editor, IEnumerable<LifestylePostTranslation> newTranslations, string newHeaderImgUrl)
    {
        return base.UpdateBy(editor, newTranslations, newHeaderImgUrl);
    }

    public override void Validate()
    {
        base.Validate();
    }
}
