namespace Blog.Services.Identity.API.Models;

public class UserRole
{
    public int Value { get; }
    public NonEmptyString Name { get; }

    private UserRole(int value, NonEmptyString name)
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

    public static UserRole FromName(NonEmptyString name)
    {
        var role = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase));

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

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not UserRole role)
            return false;

        if (ReferenceEquals(obj, this))
            return true;

        bool equalValue = role.Value.Equals(Value);
        bool equalName = string.Equals(role.Name, Name, StringComparison.OrdinalIgnoreCase);

        if (equalValue && !equalName)
            throw new InvalidDataException($"Same value ({Value}) with different names: {role.Name}, {Name}");

        if (!equalValue && equalName)
            throw new InvalidDataException($"Same name ({Name}) with different values: {role.Value}, {Value}");

        return equalName && equalValue;
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Name;
}
