using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Blog.Services.Emailing.API.Factories;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Blog.Services.Emailing.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EmailingController : ControllerBase
{
    private readonly ILogger<EmailingController> _logger;
    private readonly IEmailFactory<IFluentEmail> _emailFactory;
    private readonly ISender _sender;

    public EmailingController(
        ILogger<EmailingController> logger,
        IEmailFactory<IFluentEmail> emailFactory,
        ISender sender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailFactory = emailFactory ?? throw new ArgumentNullException(nameof(emailFactory));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    [HttpPost("send")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> SendEmailAsync([Required] Models.DTOs.SendEmailRequest)
    {
        _emailFactory.CreateCustomEmail()
        lPostsCount, result.ReturnedPostsCount, result.RemainingPostsCount);

        return Ok(result.PostPreviews);
    }
}
