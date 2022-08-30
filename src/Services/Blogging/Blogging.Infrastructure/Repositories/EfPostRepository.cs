using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Blog.Services.Blogging.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.Infrastructure.Repositories;

public class EfPostRepository : IPostRepository
{
    private readonly DbSet<PostBase> _posts;
    public IUnitOfWork UnitOfWork { get; }

    public EfPostRepository(BloggingDbContext ctx, IUnitOfWork unitOfWork)
    {
        _posts = ctx?.Posts ?? throw new ArgumentNullException(nameof(ctx));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public PostBase AddPost(PostBase post)
    {
        return _posts.Add(post).Entity;
    }

    public Task<PostBase?> FindPostAsync(PostId postId)
    {
        return _posts
            .Where(x => x.Id == postId)
            .Include(x => x.Translations)
            .SingleOrDefaultAsync();
    }
}
