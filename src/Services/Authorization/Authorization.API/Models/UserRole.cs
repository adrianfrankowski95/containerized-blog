namespace Blog.Services.Authorization.API.Models;

public sealed class UserRole
{
    public int Value { get; }
    public string Name { get; }
    private UserRole(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly UserRole Reader = new(0, nameof(Reader).ToLowerInvariant());
    public static readonly UserRole Moderator = new(1, nameof(Moderator).ToLowerInvariant());
    public static readonly UserRole Blogger = new(2, nameof(Blogger).ToLowerInvariant());
    public static readonly UserRole Administrator = new(3, nameof(Administrator).ToLowerInvariant());

    public static IEnumerable<UserRole> List() => new[] {
        Reader,
        Moderator,
        Blogger,
        Administrator };

    public static UserRole FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var role = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRole)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return role;
    }

    public static UserRole FromValue(int value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var role = List()
            .SingleOrDefault(r => r.Value == value);

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRole)} values: {string.Join(',', List().Select(r => r.Value))}. Provided value: {value}");

        return role;
    }
}
