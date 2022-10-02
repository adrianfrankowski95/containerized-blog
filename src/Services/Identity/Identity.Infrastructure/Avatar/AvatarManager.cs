using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.Infrastructure.Avatar;

public class AvatarManager : IAvatarManager
{
    private readonly IdentityDbContext _ctx;
    private readonly ISysTime _sysTime;

    public AvatarManager(IdentityDbContext ctx, ISysTime sysTime)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        _sysTime = sysTime ?? throw new ArgumentNullException(nameof(sysTime));
    }

    public ValueTask<AvatarModel?> GetAvatarAsync(Guid userId)
        => _ctx.Set<AvatarModel>().FindAsync(userId);

    public Task<int> AddOrUpdateAvatarAsync(Guid userId, byte[] imageData, string format, CancellationToken cancellationToken)
    {
        return _ctx
            .Set<AvatarModel>()
            .Upsert(new AvatarModel
            {
                UserId = userId,
                ImageData = imageData,
                Format = format,
                UpdatedAt = _sysTime.Now
            })
            .On(a => a.UserId)
            .WhenMatched(a => new AvatarModel
            {
                UserId = a.UserId,
                ImageData = imageData,
                Format = format,
                UpdatedAt = _sysTime.Now
            })
            .RunAsync(cancellationToken);
    }

    public Task DeleteAvatarAsync(Guid userId)
    {
        _ctx.Set<AvatarModel>().Remove(new AvatarModel { UserId = userId });

        return _ctx.SaveChangesAsync();
    }
}
