namespace Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;

public record TagViewModel
{
    public Guid TagId { get; init; }
    public string Language { get; init; }
    public string Value { get; init; }
    public int PostsCount { get; init; }
}
