namespace Blog.Services.Identity.API.Models;

public class ProfilePicture
{
    public const int MaxSizeBytes = 2097152; // 2 MB
    public const int MaxHeight = 200;
    public const int MaxWidth = 200;
    public Guid Id { get; }
    public byte[] ImageData { get; private set; }
    public ProfilePictureExtension Extension { get; private set; }

    public ProfilePicture(byte[] imageData, ProfilePictureExtension extension)
    {
        if (imageData is null)
            throw new ArgumentNullException(nameof(imageData));

        if (imageData.Length > MaxSizeBytes)
            throw new InvalidDataException($"The avatar picture size is too big. Maximum allowed size is {MaxSizeBytes} bytes.");

        Id = Guid.NewGuid();

        ImageData = imageData;
        Extension = extension;
    }
}

public enum ProfilePictureExtension { JPG, PNG, BMP, GIF }