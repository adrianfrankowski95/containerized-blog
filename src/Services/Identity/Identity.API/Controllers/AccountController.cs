using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Application.Queries.AvatarQueries;
using Blog.Services.Identity.API.Extensions;
using Blog.Services.Identity.API.Infrastructure.Services;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Services.Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;
    private readonly IAvatarQueries _avatarQueries;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ILogger<AccountController> logger,
        IMediator mediator,
        IIdentityService identityService,
        IAvatarQueries avatarQueries)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _avatarQueries = avatarQueries ?? throw new ArgumentNullException(nameof(avatarQueries));
    }

    [HttpGet("avatar/{username:required}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetAvatarAsync([FromRoute, Required] string username)
    {
        var avatar = await _avatarQueries.GetAvatarByUsernameAsync(username).ConfigureAwait(false);

        return avatar is null
            ? NotFound()
            : File(avatar.ImageData, "image/" + avatar.Format);
    }

    [HttpPost("avatar")]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SetOwnAvatarAsync(
        [FromForm, Required] SetOwnAvatarCommand command,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<SetOwnAvatarCommand>(id, command);

        _logger.LogSendingCommand(request);

        await _mediator.Send(request);

        var requestUri = HttpContext.GetBaseRequestUri();
        _logger.LogInformation("---- Extracted following base request Uri: {RequestUri}", requestUri);

        return Created(requestUri + '/' + _identityService?.Username?.ToString() ?? "", null);
    }

    [HttpPost("avatar/{username:required}")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.Moderator)}")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SetOtherAvatarAsync(
        [FromRoute, Required] string username,
        [FromForm, Required] IFormFile imageFile,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<SetOtherAvatarCommand>(id, new SetOtherAvatarCommand(username, imageFile));

        _logger.LogSendingCommand(request);

        await _mediator.Send(request);

        var requestUri = HttpContext.GetBaseRequestUri();
        _logger.LogInformation("---- Extracted following base request Uri: {RequestUri}", requestUri);

        return Created(requestUri, null);
    }
}
