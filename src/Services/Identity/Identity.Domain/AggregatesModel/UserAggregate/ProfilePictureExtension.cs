using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class ProfilePictureExtension : Enumeration
{
    private ProfilePictureExtension(int value, NonEmptyString name) : base(value, name)
    {
    }

    public static readonly ProfilePictureExtension JPG = new(0, nameof(JPG).ToLowerInvariant());
    public static readonly ProfilePictureExtension PNG = new(1, nameof(PNG).ToLowerInvariant());
    public static readonly ProfilePictureExtension BMP = new(2, nameof(BMP).ToLowerInvariant());
    public static readonly ProfilePictureExtension GIF = new(3, nameof(GIF).ToLowerInvariant());

    public static IEnumerable<ProfilePictureExtension> List()
        => new[] { JPG, PNG, BMP, GIF };

    public static ProfilePictureExtension FromName(NonEmptyString name)
    {
        var extension = List()
            .SingleOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        if (extension is null)
            throw new InvalidOperationException($"Possible {nameof(ProfilePictureExtension)} names: {string.Join(", ", List().Select(e => e.Name))}. Provided name: {name}");

        return extension;
    }

    public static ProfilePictureExtension FromValue(int value)
    {
        var extension = List()
            .SingleOrDefault(e => e.Value == value);

        if (extension is null)
            throw new InvalidOperationException($"Possible {nameof(ProfilePictureExtension)} values: {string.Join(',', List().Select(e => e.Value))}. Provided value: {value}");

        return extension;
    }
}