namespace Identity.API.Infrastructure.Services;

public interface IAvatarUploadService
{
    public Task UploadAvatarAsync(Guid userId, IFormFile formFile, CancellationToken cancellationToken);
}
