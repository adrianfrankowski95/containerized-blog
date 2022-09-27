namespace Blog.Services.Identity.Infrastructure.Avatar;

public interface IAvatarManager
{
    public ValueTask<AvatarModel?> GetAvatarAsync(Guid userId);
    public Task<int> AddOrUpdateAvatarAsync(Guid userId, byte[] imageData, string type, CancellationToken cancellationToken = default);
    public Task DeleteAvatarAsync(Guid userId);
}
