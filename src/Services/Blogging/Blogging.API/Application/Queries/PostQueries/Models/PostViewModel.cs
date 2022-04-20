using NodaTime;

namespace Blog.Services.Blogging.API.Application.Queries.PostQueries.Models;

public record PostPreviewModel
{
    public Guid PostId { get; init; }
    public string Language { get; init; }
    public string Category { get; init; }
    public string Type { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string AuthorName { get; init; }
    public Instant CreatedAt { get; init; }
    public IList<string> Tags { get; init; }
    public int ViewsCount { get; init; }
    public int LikesCount { get; init; }
    public int CommentsCount { get; init; }
    public string? HeaderImgUrl { get; init; }
}

public record PaginatedPostPreviewsModel
{
    public int TotalPostsCount { get; init; }
    public int ReturnedPostsCount { get; init; }
    public int RemainingPostsCount { get; init; }
    public IAsyncEnumerable<PostPreviewModel> PostPreviews { get; init; }
}

public abstract record PostViewModel
{
    public Guid PostId { get; init; }
    public string Language { get; init; }
    public string Category { get; init; }
    public string Type { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string AuthorName { get; init; }
    public Instant CreatedAt { get; init; }
    public string? EditorName { get; init; }
    public Instant? EditedAt { get; init; }
    public string Content { get; init; }
    public IList<string> Tags { get; init; }
    public int ViewsCount { get; init; }
    public int LikesCount { get; init; }
    public int CommentsCount { get; init; }
    public string? HeaderImgUrl { get; init; }
}

public record RecipePostViewModel : PostViewModel
{
    public string DishName { get; init; }
    public string Cuisine { get; init; }
    public IList<string> Ingredients { get; init; }
    public string Meal { get; init; }
    public string Difficulty { get; init; }
    public int PreparationMinutes { get; init; }
    public int CookingMinutes { get; init; }
    public int ServingsCount { get; init; }
    public string FoodComposition { get; init; }
    public string? SongUrl { get; init; }
    public IList<string> Tastes { get; init; }
    public IList<string> PreparationMethods { get; init; }
}

public abstract record ReviewPostViewModel : PostViewModel
{
    public string ReviewItemName { get; init; }
    public string? ReviewItemWebsiteUrl { get; init; }
    public int Rating { get; init; }
}

public record RestaurantReviewPostViewModel : ReviewPostViewModel
{
    public IList<string> RestaurantCuisines { get; init; }
    public string RestaurantAddressCountry { get; init; }
    public string RestaurantAddressZipCode { get; init; }
    public string RestaurantAddressCity { get; init; }
    public string RestaurantAddressStreet { get; init; }
}

public record ProductReviewPostViewModel : ReviewPostViewModel
{

}

public record LifestylePostViewModel : PostViewModel
{

}