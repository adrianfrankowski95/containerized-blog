using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate.ReviewPostAggregate;

public class RestaurantReviewPostTranslation : ReviewPostTranslationBase
{
    public Address RestaurantAddress { get; private set; }
    private readonly List<string> _restaurantCuisines;
    public IReadOnlyList<string> RestaurantCuisines => _restaurantCuisines;

    // EF Core
    private RestaurantReviewPostTranslation() { }

    public RestaurantReviewPostTranslation(
        Language language,
        string title,
        string content,
        string description,
        IEnumerable<Tag> tags,
        Address restaurantAddress,
        IEnumerable<string> restaurantCuisines)
        : base(language, title, content, description, tags)
    {
        RestaurantAddress = restaurantAddress;

        _restaurantCuisines = new(restaurantCuisines.Count());
        foreach (string cuisine in restaurantCuisines)
            AddRestaurantCuisine(cuisine);
    }

    private void AddRestaurantCuisine(string cuisine)
    {
        string trimmedCuisine = cuisine.Trim();

        if (string.IsNullOrWhiteSpace(trimmedCuisine))
            throw new BloggingDomainException($"Cuisine cannot be null or empty");

        if (!_restaurantCuisines.Contains(trimmedCuisine))
            _restaurantCuisines.Add(trimmedCuisine);
    }

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return RestaurantAddress;
        yield return RestaurantCuisines;
        yield return base.GetEqualityCheckAttributes();
    }

    public override void Validate()
    {
        if (RestaurantAddress is null)
            throw new BloggingDomainException($"{nameof(RestaurantAddress)} cannot be null");

        if (RestaurantCuisines is null)
            throw new BloggingDomainException($"{nameof(RestaurantCuisines)} cannot be null");

        if (RestaurantCuisines.Count == 0)
            throw new BloggingDomainException($"{nameof(RestaurantCuisines)} cannot be empty");

        base.Validate();
    }
}
