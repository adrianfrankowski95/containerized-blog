using Blog.Services.Blogging.API.Application.Models;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;

namespace Blog.Services.Blogging.API.Application.Queries.TagQueries;

public interface ITagQueries
{
    public IAsyncEnumerable<TagViewModel> GetTagsAsync();
    public IAsyncEnumerable<TagViewModel> GetTagsWithLanguageAsync(Language language);
}
