using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Blog.Services.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISysTime _sysTime;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger, IMediator mediator, ISysTime sysTime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    [HttpPost("avatar")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SetAvatarAsyncAsync([FromForm] SetProfilePictureCommand command)
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
}
