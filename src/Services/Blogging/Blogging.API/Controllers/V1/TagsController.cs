using System.ComponentModel.DataAnnotations;
using System.Net;
using Blog.Services.Blogging.API.Application.Queries.TagQueries;
using Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Services.Blogging.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagQueries _tagQueries;
    private readonly ILogger<PostsController> _logger;

    public TagsController(ITagQueries tagQueries, ILogger<PostsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tagQueries = tagQueries ?? throw new ArgumentNullException(nameof(tagQueries));
    }

    [HttpGet("")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<IAsyncEnumerable<TagViewModel>> GetTagsAsync()
    {
        _logger.LogInformation("----- Querying for tags");
        var result = _tagQueries.GetTagsAsync();

        return Ok(result);
    }

    [HttpGet("{language:required}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<IAsyncEnumerable<TagViewModel>> GetTagsWithLanguageAsync
    ([FromRoute, Required] string language)
    {
        var lang = Language.FromName(language);
        _logger.LogInformation("----- Querying for tags with language, parameters: {Language}", lang.Name);
        var result = _tagQueries.GetTagsWithLanguageAsync(lang);

        return Ok(result);
    }
}
