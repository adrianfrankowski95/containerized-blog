using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Blogging.API.Application.Commands;
using Blog.Services.Blogging.API.Application.Commands.Models;
using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.API.Application.Queries.PostQueries;
using Blog.Services.Blogging.API.Application.Queries.PostQueries.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Blog.Services.Blogging.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostQueries _postQueries;
    private readonly IMediator _mediator;
    private readonly ISysTime _sysTime;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostQueries postQueries, ILogger<PostsController> logger, IMediator mediator, ISysTime sysTime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _postQueries = postQueries ?? throw new ArgumentNullException(nameof(postQueries));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    [HttpGet("view")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<IAsyncEnumerable<PaginatedPostPreviewsModel>>> GetPreviewsAsync(
           [FromQuery, Required] string lang, [FromQuery] int pageSize = 10, [FromQuery] Instant? cursor = null)
    {
        cursor ??= _sysTime.Now;

        Language language = Language.FromName(lang);

        _logger.LogInformation(
            "----- Querying for paginated post previews at {UtcNow}, parameters: {PageSize}, {Cursor}, {Language}",
            DateTime.UtcNow, pageSize, cursor, language.Name);

        var result = await _postQueries.GetPublishedPaginatedPreviewsWithLanguageAsync(language, pageSize, cursor.Value)
            .ConfigureAwait(false);

        AddPostPaginationHeaders(result.TotalPostsCount, result.ReturnedPostsCount, result.RemainingPostsCount);

        return Ok(result.PostPreviews);
    }

    [HttpGet("view/{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<PostViewModel>> GetPostByIdAsync([Required] Guid id, [FromQuery, Required] string lang)
    {
        PostId postId = new(id);

        Language language = Language.FromName(lang);

        _logger.LogInformation("----- Querying for a post at {UtcNow}, parameters: {Id}, {Language}",
            DateTime.UtcNow, postId.Value, language.Name);

        PostViewModel result = await _postQueries.GetPublishedPostWithLanguageAsync(postId, language).ConfigureAwait(false);

        return Ok(result);
    }

    [HttpGet("view/{category}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<IAsyncEnumerable<PaginatedPostPreviewsModel>>> GetPreviewsWithCategoryAsync(
        [Required] string category, [FromQuery, Required] string lang, [FromQuery] int pageSize = 10, [FromQuery] Instant? cursor = null)
    {
        cursor ??= _sysTime.Now;

        Language language = Language.FromName(lang);
        PostCategory postCategory = PostCategory.FromName(category);

        _logger.LogInformation(
            "----- Querying for paginated post previews from category at {UtcNow}, parameters: {Category}, {PageSize}, {Cursor}, {Language}",
            DateTime.UtcNow, postCategory.Name, pageSize, cursor, language.Name);

        var result = await _postQueries.GetPublishedPaginatedPreviewsWithCategoryAsync(postCategory, language, pageSize, cursor.Value)
            .ConfigureAwait(false);

        AddPostPaginationHeaders(result.TotalPostsCount, result.ReturnedPostsCount, result.RemainingPostsCount);

        return Ok(result);
    }

    [Route("view/top/liked")]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public ActionResult<IAsyncEnumerable<PostPreviewModel>> GetTopLikedPreviewsAsync(
        [FromQuery, Required] string lang, [FromQuery] int count = 10, [FromQuery] int daysFromToday = 30)
    {
        Language language = Language.FromName(lang);

        _logger.LogInformation(
            "----- Querying for top-liked post previews at {UtcNow}, parameters: {PostsCount}, {DaysFromToday}, {Language}",
            DateTime.UtcNow, count, daysFromToday, language.Name);

        var result = _postQueries.GetTopLikedPublishedPreviewsWithLanguageAsync(language, count, daysFromToday);

        return Ok(result);
    }

    [Route("view/top/popular")]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public ActionResult<IAsyncEnumerable<PostPreviewModel>> GetTopPopularPreviewsAsync(
        [FromQuery, Required] string lang, [FromQuery] int count = 10, [FromQuery] int daysFromToday = 30)
    {
        Language language = Language.FromName(lang);

        _logger.LogInformation(
            "----- Querying for top-popular post previews at {UtcNow}, parameters: {PostsCount}, {DaysFromToday}, {Language}",
            DateTime.UtcNow, count, daysFromToday, language.Name);

        var result = _postQueries.GetTopPopularPublishedPreviewsWithLanguageAsync(language, count, daysFromToday);

        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpGet("{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<PostWithTranslationsViewModel>> GetPostToEditByIdAsync([Required] Guid id)
    {
        PostId postId = new(id);

        _logger.LogInformation("----- Querying for a post with translations at {UtcNow}, parameters: {Id}",
            DateTime.UtcNow, postId.Value);

        PostWithTranslationsViewModel result = await _postQueries.GetPostWithAllTranslationsAsync(postId).ConfigureAwait(false);

        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpGet("author/{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<IAsyncEnumerable<PaginatedPostPreviewsModel>> GetAllPreviewsFromAuthorAsync(
        [Required] Guid id, [FromQuery] string? lang = null)
    {
        UserId authorId = new(id);
        Language language = string.IsNullOrWhiteSpace(lang) ?
            Language.GetDefault() :
            Language.FromName(lang);

        _logger.LogInformation("----- Querying for post previews from author at {UtcNow}, parameters: {AuthorId}, {Language}",
            DateTime.UtcNow, authorId, language.Name);

        IAsyncEnumerable<PostPreviewModel> result = _postQueries.GetAllPreviewsFromAuthorAsync(authorId, language);

        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.Lifestyle)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateLifestylePostDraftAsync(
        [FromBody, Required] CreateLifestylePostDraftCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateLifestylePostDraftCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.Recipe)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateRecipePostDraftAsync(
        [FromBody, Required] CreateRecipePostDraftCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateRecipePostDraftCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.RestaurantReview)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateRestaurantReviewPostDraftAsync(
        [FromBody, Required] CreateRestaurantReviewPostDraftCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateRestaurantReviewPostDraftCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.ProductReview)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateProductReviewPostDraftAsync(
        [FromBody, Required] CreateProductReviewPostDraftCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateProductReviewPostDraftCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.Lifestyle)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateLifestylePostDraftAsync(
        [FromBody, Required] UpdateLifestylePostDraftCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.Recipe)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateRecipePostDraftAsync(
        [FromBody, Required] UpdateRecipePostDraftCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.RestaurantReview)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateRestaurantReviewPostDraftAsync(
        [FromBody, Required] UpdateRestaurantReviewPostDraftCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.ProductReview)}/draft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateProductReviewPostDraftAsync(
        [FromBody, Required] UpdateProductReviewPostDraftCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost($"{nameof(PostType.Lifestyle)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndPublishLifestylePostAsync(
        [FromBody, Required] CreateAndPublishLifestylePostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndPublishLifestylePostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost($"{nameof(PostType.Recipe)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndPublishRecipePostAsync(
        [FromBody, Required] CreateAndPublishRecipePostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndPublishRecipePostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost($"{nameof(PostType.RestaurantReview)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndPublishRestaurantReviewPostAsync(
        [FromBody, Required] CreateAndPublishRestaurantReviewPostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndPublishRestaurantReviewPostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPost($"{nameof(PostType.ProductReview)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndPublishProductReviewPostAsync(
        [FromBody, Required] CreateAndPublishProductReviewPostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndPublishProductReviewPostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"{nameof(PostType.Lifestyle)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndPublishLifestylePostAsync(
        [FromBody, Required] UpdateAndPublishLifestylePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"{nameof(PostType.Recipe)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndPublishRecipePostAsync(
        [FromBody, Required] UpdateAndPublishRecipePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"{nameof(PostType.RestaurantReview)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndPublishRestaurantReviewPostAsync(
        [FromBody, Required] UpdateAndPublishRestaurantReviewPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"{nameof(PostType.ProductReview)}/publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndPublishProductReviewPostAsync(
        [FromBody, Required] UpdateAndPublishProductReviewPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.Lifestyle)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndSubmitLifestylePostAsync(
        [FromBody, Required] CreateAndSubmitLifestylePostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndSubmitLifestylePostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.Recipe)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndSubmitRecipePostAsync(
        [FromBody, Required] CreateAndSubmitRecipePostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndSubmitRecipePostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
                request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
                request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.RestaurantReview)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndSubmitRestaurantReviewPostAsync(
        [FromBody, Required] CreateAndSubmitRestaurantReviewPostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndSubmitRestaurantReviewPostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
                request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
                request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPost($"{nameof(PostType.ProductReview)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreateAndSubmitProductReviewPostAsync(
        [FromBody, Required] CreateAndSubmitProductReviewPostCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<CreateAndSubmitProductReviewPostCommand>(id, command);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
                    request.GetType().Name, DateTime.UtcNow, request);

        var result = await _mediator.Send(request);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
                    request.GetType().Name, result, request);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.Lifestyle)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndSubmitLifestylePostAsync(
        [FromBody, Required] UpdateAndSubmitLifestylePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.Recipe)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndSubmitRecipePostAsync(
        [FromBody, Required] UpdateAndSubmitRecipePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.RestaurantReview)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndSubmitRestaurantReviewPostAsync(
        [FromBody, Required] UpdateAndSubmitRestaurantReviewPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPut($"{nameof(PostType.ProductReview)}/submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateAndSubmitProductReviewPostAsync(
        [FromBody, Required] UpdateAndSubmitProductReviewPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePostAsync(
        [FromBody, Required] DeletePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"publish")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> PublishPostAsync(
        [FromBody, Required] PublishPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Blogger))]
    [HttpPut($"submit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> SubmitPostAsync(
        [FromBody, Required] SubmitPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"approve")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ApprovePostAsync(
        [FromBody, Required] ApprovePostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Blogger))]
    [HttpPut($"setdraft")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> SetPostToDraftAsync(
        [FromBody, Required] SetPostToDraftCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    [Authorize(Roles = nameof(UserRole.Administrator))]
    [HttpPut($"reject")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> RejectPostAsync(
        [FromBody, Required] RejectPostCommand command)
    {
        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            command.GetType(), DateTime.UtcNow, command);

        var result = await _mediator.Send(command);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            command.GetType(), result, command);

        return MapCommandResult(result);
    }

    private void AddPostPaginationHeaders(int totalCount, int returnedCount, int remainingCount)
    {
        Response.Headers.Add($"x-total-post-count", totalCount.ToString());
        Response.Headers.Add($"x-remaining-post-count", remainingCount.ToString());
        Response.Headers.Add($"x-returned-post-count", returnedCount.ToString());
    }
}
