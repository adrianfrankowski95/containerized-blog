using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;

public interface ITagRepository : IRepository<Tag>
{
    public IUnitOfWork UnitOfWork { get; }
    public Task<List<Tag>> GetTagsWithLanguageAsync(Language language);
    public Task<List<Tag>> GetTagsAsync();
    public Task<List<Tag>> FindTagsByIdsAsync(IList<TagId> tagIds);
    public Tag AddTag(Tag tag);
    public Tag DeleteTag(Tag tag);
}
