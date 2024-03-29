namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.LifestylePostAggregate;

public class LifestylePost : PostBase
{
    // EF Core
    private LifestylePost() { }

    public LifestylePost(
        User author,
        IEnumerable<LifestylePostTranslation> translations,
        string headerImgUrl)
        : base(author, PostCategory.Lifestyle, PostType.Lifestyle, translations, headerImgUrl)
    { }

    public bool UpdateBy(User editor, IEnumerable<LifestylePostTranslation> newTranslations, string newHeaderImgUrl)
    {
        return base.UpdateBy(editor, newTranslations, newHeaderImgUrl);
    }

    public override void Validate()
    {
        base.Validate();
    }
}
