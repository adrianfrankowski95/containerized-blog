using Blog.Services.Blogging.API.Application.Queries.PostQueries.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using NodaTime;

namespace Blog.Services.Blogging.API.Application.Queries.PostQueries;

public interface IPostQueries
{
    public Task<PaginatedPostPreviewsModel> GetPublishedPaginatedPreviewsWithLanguageAsync(Language language, int pageSize, Instant createdAtCursor);
    public Task<PaginatedPostPreviewsModel> GetPublishedPaginatedPreviewsWithCategoryAsync(PostCategory category, Language language, int pageSize, Instant createdAtCursor);
    public IAsyncEnumerable<PostPreviewModel> GetTopPopularPublishedPreviewsWithLanguageAsync(Language language, int postsCount, int daysFromToday);
    public IAsyncEnumerable<PostPreviewModel> GetTopLikedPublishedPreviewsWithLanguageAsync(Language language, int postsCount, int daysFromToday);
    public Task<PostViewModel> GetPublishedPostWithLanguageAsync(PostId postId, Language language);
    public IAsyncEnumerable<PostPreviewModel> GetAllPreviewsFromAuthorAsync(UserId authorId, Language language);
    public Task<PostWithTranslationsViewModel> GetPostWithAllTranslationsAsync(PostId postId);
}
