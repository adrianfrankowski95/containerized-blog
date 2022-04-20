using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class RestaurantReviewPost : ReviewPostBase
{
    public Restaurant Restaurant { get; private set; }

    //ef core
    private RestaurantReviewPost() { }

    public RestaurantReviewPost(
        User author,
        IEnumerable<RestaurantReviewPostTranslation> translations,
        Restaurant restaurant,
        Rating rating,
        string headerImgUrl)
        : base(author, translations, rating, headerImgUrl)
    {
        Type = PostType.RestaurantReview;
        Restaurant = restaurant;
    }

    public virtual bool UpdateBy(
        User editor,
        IEnumerable<RestaurantReviewPostTranslation> newTranslations,
        Restaurant newRestaurant,
        Rating newRating,
        string newHeaderImgUrl)
    {
        bool isChanged = false;

        if (!Restaurant.Equals(newRestaurant))
        {
            Restaurant = newRestaurant;
            isChanged = true;
        }

        isChanged = base.UpdateBy(editor, newTranslations, newRating, newHeaderImgUrl) || isChanged;

        return isChanged;
    }

    public override void Validate()
    {
        if (Restaurant is null)
            throw new BloggingDomainException($"{nameof(Product)} cannot be null");

        Restaurant.Validate();

        base.Validate();
    }
}
