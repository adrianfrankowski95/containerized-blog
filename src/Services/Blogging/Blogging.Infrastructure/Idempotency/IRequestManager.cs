namespace Blog.Services.Blogging.Infrastructure.Idempotency;

public interface IRequestManager
{
    public Task<bool> ExistsAsync<TRequest>(Guid requestId);
    Task AddRequestAsync<TRequest>(Guid requestId);
}
