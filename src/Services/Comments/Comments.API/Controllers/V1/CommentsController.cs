using Microsoft.AspNetCore.Mvc;

namespace Blog.Services.Comments.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ILogger<CommentsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public ActionResult<IAsyncEnumerable<string>> GetAsync()
    {
        var comments = Enumerable.Range(1, 5).Select(index => $"This is a comment number {index}.");

        static async IAsyncEnumerable<string> streamAsync(IEnumerable<string> data)
        {
            foreach (var element in data)
            {
                await Task.Delay(1000);
                yield return element;
            }
        }

        return Ok(streamAsync(comments));
    }
}
