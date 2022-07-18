using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Emailing.API.Factories;
using Blog.Services.Emailing.API.Models;
using Blog.Services.Emailing.API.Models.DTOs;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> SendEmailAsync([Required, FromBody] SendEmailRequestDto requestDto)
    {
        try
        {
            var request = MapFromSendEmailRequestDto(requestDto);
            var email = _emailFactory.CreateCustomEmail(
                request.Recipients,
                request.CcRecipients,
                request.BccRecipients,
                request.Title,
                request.Body,
                request.IsBodyHtml,
                request.Priority,
                request.Attachments);

            var result = await _sender.SendAsync(email).ConfigureAwait(false);

            return result.Successful ? Ok() : Problem(string.Join(". ", result.ErrorMessages));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static SendEmailRequest MapFromSendEmailRequestDto(SendEmailRequestDto requestDto)
     => new(
            requestDto.Recipients.Select(x => new Recipient(x.EmailAddress, x.Name)),
            requestDto.CcRecipients?.Select(x => new Recipient(x.EmailAddress, x.Name)),
            requestDto.BccRecipients?.Select(x => new Recipient(x.EmailAddress, x.Name)),
            requestDto.Title,
            requestDto.Body,
            requestDto.IsBodyHtml,
            Enum.Parse<Priority>(requestDto.Priority),
            requestDto.Attachments?.Select(x => new Attachment(x.Filename, x.Data, x.ContentType, x.ContentId)));
}
