namespace Blog.Services.Identity.Infrastructure.Idempotency;

public interface IRequestManager
{
    public Task<bool> ExistsAsync<TRequest>(Guid requestId);
    public Task AddRequestAsync<TRequest>(Guid requestId);
}
