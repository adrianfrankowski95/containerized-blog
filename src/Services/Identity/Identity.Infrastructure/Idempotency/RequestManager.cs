using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.Infrastructure.Idempotency;

public class RequestManager : IRequestManager
{
    private readonly IdentityDbContext _ctx;
    private readonly ISysTime _sysTime;

    public RequestManager(IdentityDbContext ctx, ISysTime sysTime)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }
    public Task<bool> ExistsAsync<TRequest>(Guid requestId)
        => _ctx
            .Set<IdentifiedRequest>()
            .AnyAsync(x => x.Type.Equals(typeof(TRequest).Name) && x.Id.Equals(requestId));

    public async Task AddRequestAsync<TRequest>(Guid requestId)
    {
        bool exists = await ExistsAsync<TRequest>(requestId).ConfigureAwait(false);

        var request = exists
            ? throw new IdempotencyException($"Request of type {typeof(TRequest).Name} with ID {requestId} already exists.")
            : new IdentifiedRequest(requestId, typeof(TRequest).Name, _sysTime.Now);

        _ctx.Add(request);

        await _ctx.SaveChangesAsync().ConfigureAwait(false);
    }
}
