using System.Net;
using Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Microsoft.AspNetCore.Mvc;
using Blog.Services.Blogging.API.Application.Queries.TagQueries;
using Blog.Services.Blogging.Domain.Exceptions;

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

    [HttpGet("{string:language}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<IAsyncEnumerable<TagViewModel>> GetTagsAsync(string? lang = null)
    {
        IAsyncEnumerable<TagViewModel> result;
        if (string.IsNullOrWhiteSpace(lang))
        {
            _logger.LogInformation("----- Querying for tags");
            result = _tagQueries.GetTagsAsync();
        }
        else
        {
            var language = Language.FromName(lang);
            _logger.LogInformation("----- Querying for tags with language, parameters: {Language}", language.Name);
            result = _tagQueries.GetTagsWithLanguageAsync(language);
        }

        return Ok(result);
    }
}
