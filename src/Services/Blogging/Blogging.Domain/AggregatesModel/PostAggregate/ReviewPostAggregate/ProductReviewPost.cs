using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class ProductReviewPost : ReviewPostBase
{
    public Product Product { get; private set; }

    //ef core
    private ProductReviewPost() { }

    public ProductReviewPost(
        User author,
        IEnumerable<ProductReviewPostTranslation> translations,
        Product product,
        Rating rating,
        string headerImgUrl)
        : base(author, translations, rating, headerImgUrl)
    {
        Type = PostType.ProductReview;
        Product = product;
    }

    public virtual bool UpdateBy(
        User editor,
        IEnumerable<ProductReviewPostTranslation> newTranslations,
        Product newProduct,
        Rating newRating,
        string newHeaderImgUrl)
    {
        bool isChanged = false;

        if (!Product.Equals(newProduct))
        {
            Product = newProduct;
            isChanged = true;
        }

        return base.UpdateBy(editor, newTranslations, newRating, newHeaderImgUrl) || isChanged;
    }

    public override void Validate()
    {
        if (Product is null)
            throw new BloggingDomainException($"{nameof(Product)} cannot be null");

        Product.Validate();

        base.Validate();
    }
}
