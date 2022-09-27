using Blog.Services.Identity.Infrastructure.Avatar;
using SkiaSharp;

namespace Identity.API.Infrastructure.Services;

public class AvatarUploadService : IAvatarUploadService
{
    private const int MaxSizeBytes = 2097152; //2MB
    private const int RequiredHeight = 150;
    private const int RequiredWidth = 150;
    private static readonly string[] _allowedFormats = { "bmp", "png", "jpg", "gif" };
    private readonly IAvatarManager _avatarManager;

    public AvatarUploadService(IAvatarManager avatarManager)
    {
        _avatarManager = avatarManager ?? throw new ArgumentNullException(nameof(avatarManager));
    }

    public async Task UploadAvatarAsync(Guid userId, IFormFile formFile, CancellationToken cancellationToken)
    {
        if (formFile.Length > MaxSizeBytes)
            ThrowTooBig();

        using var stream = new MemoryStream();
        await formFile.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);

        if (stream.Length > MaxSizeBytes)
            ThrowTooBig();

        using var codec = SKCodec.Create(stream, out SKCodecResult result);
        if (codec is null || result is not SKCodecResult.Success)
            throw new InvalidDataException("Invalid image file.");

        string type = codec.EncodedFormat.ToString().ToLowerInvariant();

        if (!_allowedFormats.Contains(type))
            throw new InvalidDataException($"Invalid image format. Supported formats: {string.Join(", ", _allowedFormats)}.");

        if (codec.Info.Height != RequiredHeight || codec.Info.Width != RequiredWidth)
            throw new InvalidDataException($"Invalid dimensions. Supported dimensions: ${RequiredWidth}x${RequiredHeight}.");

        await _avatarManager.AddOrUpdateAvatarAsync(userId, stream.ToArray(), type, cancellationToken).ConfigureAwait(false);
    }
    private static void ThrowTooBig() => throw new InvalidDataException($"Uploaded file is too big. Maximum allowed size is {MaxSizeBytes} bytes.");
}
