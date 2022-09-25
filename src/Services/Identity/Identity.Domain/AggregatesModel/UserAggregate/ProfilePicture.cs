using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class ProfilePicture : Entity<ProfilePictureId>
{
    public const int MaxSizeBytes = 2097152; // 2 MB
    public byte[] ImageData { get; private set; }
    public ProfilePictureExtension Extension { get; private set; }

    public ProfilePicture(byte[] imageData, ProfilePictureExtension extension)
    {
        if (imageData is null)
            throw new ArgumentNullException(nameof(imageData));

        if (extension is null)
            throw new ArgumentNullException(nameof(extension));

        if (imageData.Length > MaxSizeBytes)
            throw new IdentityDomainException($"The avatar picture size is too big. Maximum allowed size is {MaxSizeBytes} bytes.");

        Id = new ProfilePictureId();

        ImageData = imageData;
        Extension = extension;
    }
}

public class ProfilePictureId : ValueObject<ProfilePictureId>
{
    public Guid Value { get; private set; }

    public ProfilePictureId()
    {
        Value = Guid.NewGuid();
    }

    public static ProfilePictureId FromGuid(Guid value) => new() { Value = value };

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}