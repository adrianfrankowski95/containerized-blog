using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class UserRole : Enumeration
{

    private UserRole(int value, NonEmptyString name) : base(value, name)
    {
    }

    public static readonly UserRole Reader = new(0, nameof(Reader).ToLowerInvariant());
    public static readonly UserRole Moderator = new(1, nameof(Moderator).ToLowerInvariant());
    public static readonly UserRole Author = new(2, nameof(Author).ToLowerInvariant());
    public static readonly UserRole Administrator = new(3, nameof(Administrator).ToLowerInvariant());

    public static UserRole DefaultRole() => Reader;

    public static IEnumerable<UserRole> List() => new[] {
        Reader,
        Moderator,
        Author,
        Administrator };

    public static UserRole FromName(NonEmptyString name)
    {
        var role = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRole)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return role;
    }

    public static UserRole FromValue(int value)
    {

        var role = List()
            .SingleOrDefault(r => r.Value == value);

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRole)} values: {string.Join(',', List().Select(r => r.Value))}. Provided value: {value}");

        return role;
    }
}
