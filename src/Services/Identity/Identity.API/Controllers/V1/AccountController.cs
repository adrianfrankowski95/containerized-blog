using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Identity.API.Application.Commands;
using Blog.Services.Identity.API.Extensions;
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

        return Created()
    }

    [HttpPost("avatar/{email:required}")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.Moderator.Name)}")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SetOtherAvatarAsync(
        [FromRoute, Required] string email,
        [FromForm, Required] IFormFile imageFile,
        [FromHeader(Name = "x-request-id"), Required] string requestId)
    {
        if (!Guid.TryParse(requestId, out Guid id))
            return BadRequest($"Incorrect or missing 'x-request-id' header");

        var request = new IdentifiedCommand<SetOtherAvatarCommand>(id, new SetOtherAvatarCommand(email, imageFile));

        _logger.LogSendingCommand(request);

        await _mediator.Send(request);

        return StatusCode((int)HttpStatusCode.Created);
    }
}
