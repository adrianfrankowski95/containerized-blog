using System.Data.Common;
using Blog.Services.Blogging.API.Application.Queries.PostQueries.Models;
using Blog.Services.Blogging.API.Extensions;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Dapper;
using NodaTime;

namespace Blog.Services.Blogging.API.Application.Queries.PostQueries;

public class DapperPostQueries : IPostQueries
{
    private readonly DbConnection _connection;

    public DapperPostQueries(DbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<PaginatedPostPreviewsModel> GetPublishedPaginatedPreviewsWithLanguageAsync(Language language, int pageSize, Instant createdAtCursor)
    {
        string totalPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM blogging.posts p
        WHERE p.status = '{PostStatus.Published.Name}' AND EXISTS(
		    SELECT 1
		    FROM blogging.post_translations AS t
		    WHERE (p.id = t.post_id) AND (t.language = @Language));";

        string olderThanPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM blogging.posts p
        WHERE p.status = '{PostStatus.Published.Name}' AND p.created_at < @Cursor AND EXISTS(
		    SELECT 1
		    FROM blogging.post_translations AS t
		    WHERE (p.id = t.post_id) AND (t.language = @Language));";

        string returnedPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM (
	        SELECT 1
	        FROM blogging.posts p
	        WHERE p.status = '{PostStatus.Published.Name}' AND p.created_at < @Cursor AND EXISTS(
		        SELECT 1
		        FROM blogging.post_translations AS t
		        WHERE (p.id = t.post_id) AND (t.language = @Language))
	        LIMIT(@PageSize)
        ) AS c;";

        string returnPostPreviewsQuery =
        @$"SELECT p.id AS {nameof(PostPreviewModel.PostId)},
            p.category AS {nameof(PostPreviewModel.Category)},
            p.type AS {nameof(PostPreviewModel.Type)},
            p.author_name AS {nameof(PostPreviewModel.AuthorName)},
            p.created_at AS {nameof(PostPreviewModel.CreatedAt)},
            p.views_count AS {nameof(PostPreviewModel.ViewsCount)},
            p.likes_count AS {nameof(PostPreviewModel.LikesCount)},
            p.comments_count AS {nameof(PostPreviewModel.CommentsCount)},
            p.header_img_url AS {nameof(PostPreviewModel.HeaderImgUrl)},
            t.language AS {nameof(PostPreviewModel.Language)},
            t.title AS {nameof(PostPreviewModel.Title)},
            t.description AS {nameof(PostPreviewModel.Description)},
            x.tags AS {nameof(PostPreviewModel.Tags)}
        FROM (
	        SELECT p0.id, p0.category, p0.author_name, p0.created_at, p0.views_count, p0.likes_count, p0.comments_count, p0.header_img_url
	        FROM blogging.posts AS p0
	        WHERE p0.status = '{PostStatus.Published.Name}' AND p0.created_at < @Cursor
        ) AS p
        INNER JOIN blogging.post_translations AS t ON (t.post_id = p.id) AND (t.language = @Language)
        LEFT JOIN (                                                                   
	        SELECT pt.post_translation_id AS translation_id, array_agg(a.value) AS tags
	            FROM blogging.post_translations_tags AS pt
	            LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id)
	            GROUP BY pt.post_translation_id
        ) AS x ON (x.translation_id = t.id)
        LIMIT (@PageSize);";

        var parameters = new { Cursor = createdAtCursor, Language = language.Name, PageSize = pageSize };

        using var counts = await _connection.QueryMultipleAsync(
            totalPostsCountQuery + olderThanPostsCountQuery + returnedPostsCountQuery,
            parameters).ConfigureAwait(false);

        var totalPostsCountTask = counts.ReadSingleAsync<int>();
        var olderThanPostsCountTask = counts.ReadSingleAsync<int>();
        var returnedPostsCountTask = counts.ReadSingleAsync<int>();

        using var postsPreviewsReader = await _connection.ExecuteReaderAsync(returnPostPreviewsQuery, parameters).ConfigureAwait(false);

        var countsResult = await Task.WhenAll(totalPostsCountTask, olderThanPostsCountTask, returnedPostsCountTask).ConfigureAwait(false);

        return new PaginatedPostPreviewsModel()
        {
            TotalPostsCount = countsResult[0],
            ReturnedPostsCount = countsResult[2],
            RemainingPostsCount = countsResult[1] - countsResult[2],
            PostPreviews = postsPreviewsReader.StreamAsync<PostPreviewModel>()
        };
    }

    public async Task<PaginatedPostPreviewsModel> GetPublishedPaginatedPreviewsWithCategoryAsync(PostCategory category, Language language, int pageSize, Instant createdAtCursor)
    {
        string totalPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM blogging.posts p
        WHERE p.status = '{PostStatus.Published.Name}' AND p.category = @Category AND EXISTS(
		    SELECT 1
		    FROM blogging.post_translations AS t
		    WHERE (p.id = t.post_id) AND (t.language = @Language));";

        string olderThanPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM blogging.posts p
        WHERE p.status = '{PostStatus.Published.Name}' AND created_at < @Cursor AND p.category = @Category AND EXISTS(
		    SELECT 1
		    FROM blogging.post_translations AS t
		    WHERE (p.id = t.post_id) AND (t.language = @Language));";

        string returnedPostsCountQuery =
        @$"SELECT COUNT(*)
        FROM (
	        SELECT 1
	        FROM blogging.posts p
	        WHERE p.status = '{PostStatus.Published.Name}' AND created_at < @Cursor AND p.category = @Category AND EXISTS(
		        SELECT 1
		        FROM blogging.post_translations AS t
		        WHERE (p.id = t.post_id) AND (t.language = @Language))
	        LIMIT(@PageSize)
        ) AS p;";

        string returnPostPreviewsQuery =
        @$"SELECT p.id AS {nameof(PostPreviewModel.PostId)},
            p.category AS {nameof(PostPreviewModel.Category)},
            p.type AS {nameof(PostPreviewModel.Type)},
            p.author_name AS {nameof(PostPreviewModel.AuthorName)},
            p.created_at AS {nameof(PostPreviewModel.CreatedAt)},
            p.views_count AS {nameof(PostPreviewModel.ViewsCount)},
            p.likes_count AS {nameof(PostPreviewModel.LikesCount)},
            p.comments_count AS {nameof(PostPreviewModel.CommentsCount)},
            p.header_img_url AS {nameof(PostPreviewModel.HeaderImgUrl)},
            t.language AS {nameof(PostPreviewModel.Language)},
            t.title AS {nameof(PostPreviewModel.Title)},
            t.description AS {nameof(PostPreviewModel.Description)},
            x.tags AS {nameof(PostPreviewModel.Tags)}
        FROM (
	        SELECT p0.id, p0.category, p0.author_name, p0.created_at, p0.views_count, p0.likes_count, p0.comments_count, p0.header_img_url
	        FROM blogging.posts AS p0
	        WHERE p0.status = '{PostStatus.Published.Name}' AND p0.created_at < @Cursor AND p0.category = @Category
        ) AS p
        INNER JOIN blogging.post_translations AS t ON (t.post_id = p.id) AND (t.language = @Language)
        LEFT JOIN (                                                                   
	        SELECT pt.post_translation_id AS translation_id, array_agg(a.value) AS tags
	            FROM blogging.post_translations_tags AS pt
	            LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id)
	            GROUP BY pt.post_translation_id
        ) AS x ON (x.translation_id = t.id)
        LIMIT (@PageSize);";

        var parameters = new { Category = category.Name, Cursor = createdAtCursor, Language = language.Name, PageSize = pageSize };

        using var counts = await _connection.QueryMultipleAsync(
            totalPostsCountQuery + olderThanPostsCountQuery + returnedPostsCountQuery,
            parameters).ConfigureAwait(false);

        var totalPostsCountTask = counts.ReadSingleAsync<int>();
        var olderThanPostsCountTask = counts.ReadSingleAsync<int>();
        var returnedPostsCountTask = counts.ReadSingleAsync<int>();

        using var postsPreviewsReader = await _connection.ExecuteReaderAsync(returnPostPreviewsQuery, parameters).ConfigureAwait(false);

        var countsResult = await Task.WhenAll(totalPostsCountTask, olderThanPostsCountTask, returnedPostsCountTask).ConfigureAwait(false);

        return new PaginatedPostPreviewsModel()
        {
            TotalPostsCount = countsResult[0],
            ReturnedPostsCount = countsResult[2],
            RemainingPostsCount = countsResult[1] - countsResult[2],
            PostPreviews = postsPreviewsReader.StreamAsync<PostPreviewModel>()
        };
    }

    public async IAsyncEnumerable<PostPreviewModel> GetTopLikedPublishedPreviewsWithLanguageAsync(Language language, int postsCount, int daysFromToday)
    {
        string returnPostPreviewsQuery =
        @$"SELECT p.id AS {nameof(PostPreviewModel.PostId)},
            p.category AS {nameof(PostPreviewModel.Category)},
            p.type AS {nameof(PostPreviewModel.Type)},
            p.author_name AS {nameof(PostPreviewModel.AuthorName)},
            p.created_at AS {nameof(PostPreviewModel.CreatedAt)},
            p.views_count AS {nameof(PostPreviewModel.ViewsCount)},
            p.likes_count AS {nameof(PostPreviewModel.LikesCount)},
            p.comments_count AS {nameof(PostPreviewModel.CommentsCount)},
            p.header_img_url AS {nameof(PostPreviewModel.HeaderImgUrl)},
            t.language AS {nameof(PostPreviewModel.Language)},
            t.title AS {nameof(PostPreviewModel.Title)},
            t.description AS {nameof(PostPreviewModel.Description)},
            x.tags AS {nameof(PostPreviewModel.Tags)}
        FROM (
	        SELECT p0.id, p0.category, p0.author_name, p0.created_at, p0.views_count, p0.likes_count, p0.comments_count, p0.header_img_url
	        FROM blogging.posts AS p0
	        WHERE (p0.status = '{PostStatus.Published.Name}') AND (EXTRACT (DAY FROM NOW() - p0.created_at) <= @DaysFromToday)
        ) AS p
        INNER JOIN blogging.post_translations AS t ON (t.post_id = p.id) AND (t.language = @Language)
        LEFT JOIN (                                                                   
	        SELECT pt.post_translation_id AS translation_id, array_agg(a.value) AS tags
	            FROM blogging.post_translations_tags AS pt
	            LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id)
	            GROUP BY pt.post_translation_id
        ) AS x ON (x.translation_id = t.id)
        ORDER BY p.likes_count, p.created_at DESC
        LIMIT (@PostsCount);";

        var parameters = new { PostsCount = postsCount, Language = language.Name, DaysFromToday = daysFromToday };

        using var postsPreviewsReader = await _connection.ExecuteReaderAsync(returnPostPreviewsQuery, parameters).ConfigureAwait(false);

        var rowParser = postsPreviewsReader.GetRowParser<PostPreviewModel>();

        while (await postsPreviewsReader.ReadAsync().ConfigureAwait(false))
            yield return rowParser(postsPreviewsReader);
    }

    public async IAsyncEnumerable<PostPreviewModel> GetTopPopularPublishedPreviewsWithLanguageAsync(Language language, int postsCount, int daysFromToday)
    {
        string returnPostPreviewsQuery =
        @$"SELECT p.id AS {nameof(PostPreviewModel.PostId)},
            p.category AS {nameof(PostPreviewModel.Category)},
            p.type AS {nameof(PostPreviewModel.Type)},
            p.author_name AS {nameof(PostPreviewModel.AuthorName)},
            p.created_at AS {nameof(PostPreviewModel.CreatedAt)},
            p.views_count AS {nameof(PostPreviewModel.ViewsCount)},
            p.likes_count AS {nameof(PostPreviewModel.LikesCount)},
            p.comments_count AS {nameof(PostPreviewModel.CommentsCount)},
            p.header_img_url AS {nameof(PostPreviewModel.HeaderImgUrl)},
            t.language AS {nameof(PostPreviewModel.Language)},
            t.title AS {nameof(PostPreviewModel.Title)},
            t.description AS {nameof(PostPreviewModel.Description)},
            x.tags AS {nameof(PostPreviewModel.Tags)}
        FROM (
	        SELECT p0.id, p0.category, p0.author_name, p0.created_at, p0.views_count, p0.likes_count, p0.comments_count, p0.header_img_url
	        FROM blogging.posts AS p0
	        WHERE (p0.status = '{PostStatus.Published.Name}') AND (EXTRACT (DAY FROM NOW() - p0.created_at) <= @DaysFromToday)
        ) AS p
        INNER JOIN blogging.post_translations AS t ON (t.post_id = p.id) AND (t.language = @Language)
        INNER JOIN (                                                                   
	        SELECT pt.post_translation_id AS translation_id, array_agg(a.value) AS tags
	            FROM blogging.post_translations_tags AS pt
	            INNER JOIN blogging.tags AS a ON (pt.tag_id = a.id)
	            GROUP BY pt.post_translation_id
        ) AS x ON (x.translation_id = t.id)
        ORDER BY p.views_count, p.created_at DESC
        LIMIT (@PostsCount);";

        var parameters = new { PostsCount = postsCount, Language = language.Name, DaysFromToday = daysFromToday };

        using var postsPreviewsReader = await _connection.ExecuteReaderAsync(returnPostPreviewsQuery, parameters).ConfigureAwait(false);

        await foreach (var postPreview in postsPreviewsReader.StreamAsync<PostPreviewModel>().ConfigureAwait(false))
            yield return postPreview;
    }

    public async IAsyncEnumerable<PostPreviewModel> GetAllPreviewsFromAuthorAsync(UserId authorId, Language language)
    {
        string returnPostPreviewsQuery =
        @$"SELECT p.id AS {nameof(PostPreviewModel.PostId)},
            p.category AS {nameof(PostPreviewModel.Category)},
            p.type AS {nameof(PostPreviewModel.Type)},
            p.author_name AS {nameof(PostPreviewModel.AuthorName)},
            p.created_at AS {nameof(PostPreviewModel.CreatedAt)},
            p.views_count AS {nameof(PostPreviewModel.ViewsCount)},
            p.likes_count AS {nameof(PostPreviewModel.LikesCount)},
            p.comments_count AS {nameof(PostPreviewModel.CommentsCount)},
            p.header_img_url AS {nameof(PostPreviewModel.HeaderImgUrl)},
            t.language AS {nameof(PostPreviewModel.Language)},
            t.title AS {nameof(PostPreviewModel.Title)},
            t.description AS {nameof(PostPreviewModel.Description)},
            x.tags AS {nameof(PostPreviewModel.Tags)}
        FROM (
	        SELECT p0.id, p0.category, p0.author_name, p0.created_at, p0.views_count, p0.likes_count, p0.comments_count, p0.header_img_url
	        FROM blogging.posts AS p0
	        WHERE (p0.author_id = @AuthorId)
        ) AS p
        LEFT JOIN blogging.post_translations AS t ON (t.post_id = p.id)
            AND (
                EXISTS (
                    SELECT 1
                    FROM blogging.post_translations AS t0
                    WHERE (p.id = t0.post_id) AND (t0.language = @Language)
                )
				AND
                    t.language = @Language	
                OR
				    t.language = @DefaultLanguage
            )
        LEFT JOIN (                                                                   
	        SELECT pt.post_translation_id AS translation_id, array_agg(a.value) AS tags
	            FROM blogging.post_translations_tags AS pt
	            LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id)
	            GROUP BY pt.post_translation_id
        ) AS x ON (x.translation_id = t.id)
        LIMIT (@PostsCount);";

        var parameters = new { AuthorId = authorId.Value, Language = language.Name, DefaultLanguage = Language.GetDefault().Name };

        using var postsPreviewsReader = await _connection.ExecuteReaderAsync(returnPostPreviewsQuery, parameters).ConfigureAwait(false);

        var rowParser = postsPreviewsReader.GetRowParser<PostPreviewModel>();

        while (await postsPreviewsReader.ReadAsync().ConfigureAwait(false))
            yield return rowParser(postsPreviewsReader);
    }

    public async Task<PostWithTranslationsViewModel> GetPostWithAllTranslationsAsync(PostId postId)
    {
        string returnPostWithTranslationsQuery =
            @$"SELECT p.id AS {nameof(PostWithTranslationsViewModel.PostId)},
                p.category AS {nameof(PostWithTranslationsViewModel.Category)},
                p.type AS {nameof(PostWithTranslationsViewModel.Type)},
                p.author_name AS {nameof(PostWithTranslationsViewModel.AuthorName)},
                p.created_at AS {nameof(PostWithTranslationsViewModel.CreatedAt)},
                p.editor_name AS {nameof(PostWithTranslationsViewModel.EditorName)},
                p.edited_at AS {nameof(PostWithTranslationsViewModel.EditedAt)},
                p.views_count AS {nameof(PostWithTranslationsViewModel.ViewsCount)},
                p.likes_count AS {nameof(PostWithTranslationsViewModel.LikesCount)},
                p.comments_count AS {nameof(PostWithTranslationsViewModel.CommentsCount)},
                p.header_img_url AS {nameof(PostWithTranslationsViewModel.HeaderImgUrl)},
                p.recipe_meal AS {nameof(RecipePostWithTranslationsViewModel.Meal)},      
                p.recipe_difficulty AS {nameof(RecipePostWithTranslationsViewModel.Difficulty)},
                p.recipe_preparation_minutes AS {nameof(RecipePostWithTranslationsViewModel.PreparationMinutes)},
                p.recipe_cooking_minutes AS {nameof(RecipePostWithTranslationsViewModel.CookingMinutes)},
                p.recipe_servings_count AS {nameof(RecipePostWithTranslationsViewModel.ServingsCount)},
                p.recipe_food_composition AS {nameof(RecipePostWithTranslationsViewModel.FoodComposition)},     
                p.recipe_preparation_methods AS {nameof(RecipePostWithTranslationsViewModel.PreparationMethods)},
                p.recipe_tastes AS {nameof(RecipePostWithTranslationsViewModel.Tastes)},
                p.recipe_song_url AS {nameof(RecipePostWithTranslationsViewModel.SongUrl)},
                p.review_item_name AS {nameof(ReviewPostWithTranslationsViewModel.ReviewItemName)},
                p.review_item_website_url AS {nameof(ReviewPostWithTranslationsViewModel.ReviewItemWebsiteUrl)},
                p.review_rating AS {nameof(ReviewPostWithTranslationsViewModel.Rating)},
                t.title AS {nameof(PostTranslationViewModel.Title)},
                t.content AS {nameof(PostTranslationViewModel.Content)},
                t.description AS {nameof(PostTranslationViewModel.Description)},
                t.language AS {nameof(PostTranslationViewModel.Language)},
                t.tags AS {nameof(PostTranslationViewModel.Tags)},
                t.recipe_dish_name AS {nameof(RecipePostTranslationViewModel.DishName)},
                t.recipe_cuisine AS {nameof(RecipePostTranslationViewModel.Cuisine)},
                t.recipe_ingredients AS {nameof(RecipePostTranslationViewModel.Ingredients)},
                t.review_restaurant_country AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressCountry)},
                t.review_restaurant_zipcode AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressZipCode)},
                t.review_restaurant_city AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressCity)},
                t.review_restaurant_street AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressStreet)},
                t.review_restaurant_cuisines AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantCuisines)}          
            FROM blogging.posts AS p
            LEFT JOIN (
                SELECT t.id,
                    t.post_id,
                    t.title,
                    t.content,
                    t.description,
                    t.recipe_dish_name,
                    t.recipe_cuisine,
                    t.recipe_ingredients, 
                    t.review_restaurant_country,
                    t.review_restaurant_zipcode,
                    t.review_restaurant_city,
                    t.review_restaurant_street, 
                    t.review_restaurant_cuisines,
                    t.language,
                    array_agg(a.value) AS tags
                FROM blogging.post_translations_tags AS pt
                LEFT JOIN blogging.post_translations AS t ON t.post_id = @PostId
                LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id) AND (t.language = a.language)
                WHERE (pt.post_translation_id = t.id) AND (pt.tag_id = a.id) 
                GROUP BY t.id, t.post_id, t.title, t.content, t.description, t.recipe_dish_name, t.recipe_cuisine, t.recipe_ingredients, 
                    t.review_restaurant_country, t.review_restaurant_zipcode, t.review_restaurant_city, t.review_restaurant_street,
                    t.review_restaurant_cuisines, t.language
                ) AS t ON (t.post_id = p.id)
            WHERE p.id = @PostId";

        var parameters = new { PostId = postId.Value };

        var result = await _connection.QueryAsync<dynamic>(returnPostWithTranslationsQuery, parameters).ConfigureAwait(false);

        if (result is null || result.AsList().Count == 0)
            throw new KeyNotFoundException($"Could not find a post with the following ID: {postId.Value}");

        return MapToViewModelWithTranslations(result);
    }

    public async Task<PostViewModel> GetPublishedPostWithLanguageAsync(PostId postId, Language language)
    {
        string returnPostQuery =
            @$"SELECT p.id AS {nameof(PostViewModel.PostId)},
                p.category AS {nameof(PostViewModel.Category)},
                p.type AS {nameof(PostWithTranslationsViewModel.Type)},
                p.author_name AS {nameof(PostViewModel.AuthorName)},
                p.created_at AS {nameof(PostViewModel.CreatedAt)},
                p.editor_name AS {nameof(PostViewModel.EditorName)},
                p.edited_at AS {nameof(PostViewModel.EditedAt)},
                p.views_count AS {nameof(PostViewModel.ViewsCount)},
                p.likes_count AS {nameof(PostViewModel.LikesCount)},
                p.comments_count AS {nameof(PostViewModel.CommentsCount)},
                p.header_img_url AS {nameof(PostViewModel.HeaderImgUrl)},
                p.recipe_meal AS {nameof(RecipePostViewModel.Meal)},      
                p.recipe_difficulty AS {nameof(RecipePostViewModel.Difficulty)},
                p.recipe_preparation_minutes AS {nameof(RecipePostViewModel.PreparationMinutes)},
                p.recipe_cooking_minutes AS {nameof(RecipePostViewModel.CookingMinutes)},
                p.recipe_servings_count AS {nameof(RecipePostViewModel.ServingsCount)},
                p.recipe_food_composition AS {nameof(RecipePostViewModel.FoodComposition)},     
                p.recipe_preparation_methods AS {nameof(RecipePostViewModel.PreparationMethods)},
                p.recipe_tastes AS {nameof(RecipePostViewModel.Tastes)},
                p.recipe_song_url AS {nameof(RecipePostViewModel.SongUrl)},
                p.review_item_name AS {nameof(ReviewPostViewModel.ReviewItemName)},
                p.review_item_website_url AS {nameof(ReviewPostViewModel.ReviewItemWebsiteUrl)},
                p.review_rating AS {nameof(ReviewPostViewModel.Rating)},
                t.title AS {nameof(PostTranslationViewModel.Title)},
                t.content AS {nameof(PostTranslationViewModel.Content)},
                t.description AS {nameof(PostTranslationViewModel.Description)},
                t.language AS {nameof(PostTranslationViewModel.Language)},
                t.tags AS {nameof(PostTranslationViewModel.Tags)},
                t.recipe_dish_name AS {nameof(RecipePostTranslationViewModel.DishName)},
                t.recipe_cuisine AS {nameof(RecipePostTranslationViewModel.Cuisine)},
                t.recipe_ingredients AS {nameof(RecipePostTranslationViewModel.Ingredients)},
                t.review_restaurant_country AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressCountry)},
                t.review_restaurant_zipcode AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressZipCode)},
                t.review_restaurant_city AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressCity)},
                t.review_restaurant_street AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantAddressStreet)},
                t.review_restaurant_cuisines AS {nameof(RestaurantReviewPostTranslationViewModel.RestaurantCuisines)}          
            FROM blogging.posts AS p
            INNER JOIN (
                SELECT t.id,
                    t.post_id,
                    t.title,
                    t.content,
                    t.description,
                    t.recipe_dish_name,
                    t.recipe_cuisine,
                    t.recipe_ingredients, 
                    t.review_restaurant_country,
                    t.review_restaurant_zipcode,
                    t.review_restaurant_city,
                    t.review_restaurant_street, 
                    t.review_restaurant_cuisines,
                    t.language,
                    array_agg(a.value) AS tags
                FROM blogging.post_translations_tags AS pt
                INNER JOIN blogging.post_translations AS t ON (t.post_id = @PostId) AND (t.language = @Language)
                LEFT JOIN blogging.tags AS a ON (pt.tag_id = a.id) AND (t.language = a.language)
                WHERE (pt.post_translation_id = t.id) AND (pt.tag_id = a.id) 
                GROUP BY t.id, t.post_id, t.title, t.content, t.description, t.recipe_dish_name, t.recipe_cuisine, t.recipe_ingredients, 
                    t.review_restaurant_country, t.review_restaurant_zipcode, t.review_restaurant_city, t.review_restaurant_street,
                    t.review_restaurant_cuisines, t.language
                ) AS t ON (t.post_id = p.id) AND (t.language = @Language)
            WHERE p.id = @PostId";

        var parameters = new { PostId = postId.Value, Language = language.Name };

        var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(returnPostQuery, parameters).ConfigureAwait(false);

        if (result is null)
            throw new KeyNotFoundException($"Could not find a post with the following ID: {postId.Value}");

        return MapToViewModel(result);
    }

    private static PostWithTranslationsViewModel MapToViewModelWithTranslations(dynamic result)
    {
        string postTypeString = (string)result[0].type;

        var postType = PostType.FromName(postTypeString);

        switch (postType)
        {
            case PostType when postType.Equals(PostType.Recipe):
                var recipePost = new RecipePostWithTranslationsViewModel()
                {
                    PostId = result[0].postid,
                    Category = result[0].category,
                    Type = result[0].type,
                    AuthorName = result[0].authorname,
                    CreatedAt = result[0].createdat,
                    EditorName = result[0].editorname,
                    EditedAt = result[0].editedat,
                    ViewsCount = result[0].viewscount,
                    LikesCount = result[0].likescount,
                    CommentsCount = result[0].commentscount,
                    HeaderImgUrl = result[0].headerimgurl,
                    Translations = new(),

                    Meal = result[0].meal,
                    Difficulty = result[0].difficulty,
                    PreparationMethods = result[0].preparationmethods,
                    PreparationMinutes = result[0].preparationminutes,
                    CookingMinutes = result[0].cookingminutes,
                    ServingsCount = result[0].servingscount,
                    FoodComposition = result[0].foodcomposition,
                    SongUrl = result[0].songurl,
                    Tastes = result[0].tastes,
                };

                foreach (dynamic translation in result)
                {
                    var trns = new RecipePostTranslationViewModel()
                    {
                        Language = translation.language,
                        Title = translation.title,
                        Content = translation.content,
                        Description = translation.description,
                        Tags = translation.tags,

                        DishName = translation.dishName,
                        Cuisine = translation.cuisine,
                        Ingredients = translation.ingredients
                    };

                    recipePost.Translations.Add(trns);
                }
                return recipePost;

            case PostType when postType.Equals(PostType.Lifestyle):

                var lifestylePost = new LifestylePostWithTranslationsViewModel()
                {
                    PostId = result[0].postid,
                    Category = result[0].category,
                    Type = result[0].type,
                    AuthorName = result[0].authorname,
                    CreatedAt = result[0].createdat,
                    EditorName = result[0].editorname,
                    EditedAt = result[0].editedat,
                    ViewsCount = result[0].viewscount,
                    LikesCount = result[0].likescount,
                    CommentsCount = result[0].commentscount,
                    HeaderImgUrl = result[0].headerimgurl,
                    Translations = new(),
                };

                foreach (dynamic translation in result)
                {
                    var trns = new LifestylePostTranslationViewModel()
                    {
                        Language = translation.language,
                        Title = translation.title,
                        Content = translation.content,
                        Description = translation.description,
                        Tags = translation.tags,
                    };

                    lifestylePost.Translations.Add(trns);
                }
                return lifestylePost;

            case PostType when postType.Equals(PostType.RestaurantReview):
                var restaurantReviewPost = new RestaurantReviewPostWithTranslationsViewModel()
                {
                    PostId = result[0].postid,
                    Category = result[0].category,
                    Type = result[0].type,
                    AuthorName = result[0].authorname,
                    CreatedAt = result[0].createdat,
                    EditorName = result[0].editorname,
                    EditedAt = result[0].editedat,
                    ViewsCount = result[0].viewscount,
                    LikesCount = result[0].likescount,
                    CommentsCount = result[0].commentscount,
                    HeaderImgUrl = result[0].headerimgurl,
                    Translations = new(),

                    ReviewItemName = result[0].reviewitemname,
                    ReviewItemWebsiteUrl = result[0].reviewitemwebsiteurl,
                    Rating = result[0].rating
                };

                foreach (dynamic translation in result)
                {
                    var trns = new RestaurantReviewPostTranslationViewModel()
                    {
                        Language = translation.language,
                        Title = translation.title,
                        Content = translation.content,
                        Description = translation.description,
                        Tags = translation.tags,

                        RestaurantCuisines = translation.restaurantcuisines,
                        RestaurantAddressCity = translation.restaurantaddresscity,
                        RestaurantAddressCountry = translation.restaurantaddresscountry,
                        RestaurantAddressStreet = translation.restaurantaddressstreet,
                        RestaurantAddressZipCode = translation.restaurantaddresszipcode
                    };

                    restaurantReviewPost.Translations.Add(trns);
                }
                return restaurantReviewPost;

            case PostType when postType.Equals(PostType.ProductReview):
                var productReviewPost = new ProductReviewPostWithTranslationsViewModel()
                {
                    PostId = result[0].postid,
                    Category = result[0].category,
                    Type = result[0].type,
                    AuthorName = result[0].authorname,
                    CreatedAt = result[0].createdat,
                    EditorName = result[0].editorname,
                    EditedAt = result[0].editedat,
                    ViewsCount = result[0].viewscount,
                    LikesCount = result[0].likescount,
                    CommentsCount = result[0].commentscount,
                    HeaderImgUrl = result[0].headerimgurl,
                    Translations = new(),

                    ReviewItemName = result[0].reviewitemname,
                    ReviewItemWebsiteUrl = result[0].reviewitemwebsiteurl,
                    Rating = result[0].rating
                };

                foreach (dynamic translation in result)
                {
                    var trns = new ProductReviewPostTranslationViewModel()
                    {
                        Language = translation.language,
                        Title = translation.title,
                        Content = translation.content,
                        Description = translation.description,
                        Tags = translation.tags,
                    };

                    productReviewPost.Translations.Add(trns);
                }
                return productReviewPost;

            default:
                throw new NotSupportedException($"Unrecognized {nameof(PostType)}: {postType}");
        };
    }

    private static PostViewModel MapToViewModel(dynamic result)
    {
        string postTypeString = (string)result[0].type;

        var postType = PostType.FromName(postTypeString);

        switch (postType)
        {
            case PostType when postType.Equals(PostType.Recipe):
                var recipePost = new RecipePostViewModel()
                {
                    PostId = result.postid,
                    Category = result.category,
                    Type = result.type,
                    AuthorName = result.authorname,
                    CreatedAt = result.createdat,
                    EditorName = result.editorname,
                    EditedAt = result.editedat,
                    ViewsCount = result.viewscount,
                    LikesCount = result.likescount,
                    CommentsCount = result.commentscount,
                    HeaderImgUrl = result.headerimgurl,

                    Meal = result.meal,
                    Difficulty = result.difficulty,
                    PreparationMethods = result.preparationmethods,
                    PreparationMinutes = result.preparationminutes,
                    CookingMinutes = result.cookingminutes,
                    ServingsCount = result.servingscount,
                    FoodComposition = result.foodcomposition,
                    SongUrl = result.songurl,
                    Tastes = result.tastes,

                    Language = result.language,
                    Title = result.title,
                    Content = result.content,
                    Description = result.description,
                    Tags = result.tags,

                    DishName = result.dishname,
                    Cuisine = result.cuisine,
                    Ingredients = result.ingredients
                };

                return recipePost;

            case PostType when postType.Equals(PostType.Lifestyle):

                var lifestylePost = new LifestylePostViewModel()
                {
                    PostId = result.postid,
                    Category = result.category,
                    Type = result.type,
                    AuthorName = result.authorname,
                    CreatedAt = result.createdat,
                    EditorName = result.editorname,
                    EditedAt = result.editedat,
                    ViewsCount = result.viewscount,
                    LikesCount = result.likescount,
                    CommentsCount = result.commentscount,
                    HeaderImgUrl = result.headerimgurl,

                    Language = result.language,
                    Title = result.title,
                    Content = result.content,
                    Description = result.description,
                    Tags = result.tags,
                };

                return lifestylePost;

            case PostType when postType.Equals(PostType.RestaurantReview):

                var restaurantReviewPost = new RestaurantReviewPostViewModel()
                {
                    PostId = result.postid,
                    Category = result.category,
                    Type = result.type,
                    AuthorName = result.authorname,
                    CreatedAt = result.createdat,
                    EditorName = result.editorname,
                    EditedAt = result.editedat,
                    ViewsCount = result.viewscount,
                    LikesCount = result.likescount,
                    CommentsCount = result.commentscount,
                    HeaderImgUrl = result.headerimgurl,

                    ReviewItemName = result.reviewitemname,
                    ReviewItemWebsiteUrl = result.reviewitemwebsiteurl,
                    Rating = result.rating,

                    Language = result.language,
                    Title = result.title,
                    Content = result.content,
                    Description = result.description,
                    Tags = result.tags,

                    RestaurantCuisines = result.restaurantcuisines,
                    RestaurantAddressCity = result.restaurantaddresscity,
                    RestaurantAddressCountry = result.restaurantaddresscountry,
                    RestaurantAddressStreet = result.restaurantaddressstreet,
                    RestaurantAddressZipCode = result.restaurantaddresszipcode
                };
                return restaurantReviewPost;

            case PostType when postType.Equals(PostType.RestaurantReview):

                var productReviewPost = new ProductReviewPostViewModel()
                {
                    PostId = result.postid,
                    Category = result.category,
                    Type = result.type,
                    AuthorName = result.authorname,
                    CreatedAt = result.createdat,
                    EditorName = result.editorname,
                    EditedAt = result.editedat,
                    ViewsCount = result.viewscount,
                    LikesCount = result.likescount,
                    CommentsCount = result.commentscount,
                    HeaderImgUrl = result.headerimgurl,

                    ReviewItemName = result.reviewitemname,
                    ReviewItemWebsiteUrl = result.reviewitemwebsiteurl,
                    Rating = result.Rating,

                    Language = result.language,
                    Title = result.title,
                    Content = result.content,
                    Description = result.description,
                    Tags = result.tags,
                };

                return productReviewPost;

            default:
                throw new NotSupportedException($"Unrecognized {nameof(PostType)}: {postType}");
        };
    }
}