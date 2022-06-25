using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.Infrastructure.Repositories;

public class EfTagRepository : ITagRepository
{
    private readonly DbSet<Tag> _tags;
    public IUnitOfWork UnitOfWork { get; }

    public EfTagRepository(BloggingDbContext ctx, IUnitOfWork unitOfWork)
    {
        _tags = ctx.Tags ?? throw new ArgumentNullException(nameof(ctx));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Tag AddTag(Tag tag)
    {
        return _tags.Add(tag).Entity;
    }

    public Tag DeleteTag(Tag tag)
    {
        return _tags.Remove(tag).Entity;
    }

    public Task<List<Tag>> GetTagsAsync()
    {
        return _tags.ToListAsync();
    }

    public Task<List<Tag>> GetTagsWithLanguageAsync(Language language)
    {
        return _tags.Where(x => x.Language.Equals(language)).ToListAsync();
    }

    public Task<List<Tag>> FindTagsByIdsAsync(IList<TagId> tagIds)
    {
        return _tags.Where(x => tagIds.Contains(x.Id)).ToListAsync();
    }
}
