using Blog.Services.Blogging.Domain.SeedWork;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public interface IPostRepository : IRepository<PostBase>
{
    public IUnitOfWork UnitOfWork { get; }
    public Task<PostBase?> FindPostAsync(PostId postId);
    public PostBase AddPost(PostBase post);
}
