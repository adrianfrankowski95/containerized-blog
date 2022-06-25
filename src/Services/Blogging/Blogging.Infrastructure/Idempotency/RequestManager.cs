using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.Infrastructure.Idempotency;

public class RequestManager : IRequestManager
{
    private readonly BloggingDbContext _ctx;
    private readonly ISysTime _sysTime;

    public RequestManager(BloggingDbContext ctx, ISysTime sysTime)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }
    public async Task<bool> ExistsAsync<TRequest>(Guid requestId)
    {
        var request = await _ctx.Set<IdentifiedRequest>()
            .Where(x => x.Type.Equals(typeof(TRequest).Name) && x.Id.Equals(requestId))
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        return request is not null;
    }

    public async Task AddRequestAsync<TRequest>(Guid requestId)
    {
        bool exists = await ExistsAsync<TRequest>(requestId).ConfigureAwait(false);

        var request = exists ?
            throw new BloggingDomainException($"Request of type {typeof(TRequest).Name} with ID {requestId} already exists") :
            new IdentifiedRequest(requestId, typeof(TRequest).Name, _sysTime.Now);

        _ctx.Add(request);

        await _ctx.SaveChangesAsync().ConfigureAwait(false);
    }
}
