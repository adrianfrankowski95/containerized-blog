namespace Blog.Services.Identity.API.Models;

public sealed class Role
{
    public int Value { get; }
    public NonEmptyString Name { get; }

    private Role(int value, NonEmptyString name)
    {
        Value = value;
        Name = name;
    }

    public static readonly Role Reader = new(0, nameof(Reader).ToLowerInvariant());
    public static readonly Role Moderator = new(1, nameof(Moderator).ToLowerInvariant());
    public static readonly Role Blogger = new(2, nameof(Blogger).ToLowerInvariant());
    public static readonly Role Administrator = new(3, nameof(Administrator).ToLowerInvariant());

    public static IEnumerable<Role> List() => new[] {
        Reader,
        Moderator,
        Blogger,
        Administrator };

    public static Role FromName(NonEmptyString name)
    {
        var role = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(Role)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return role;
    }

    public static Role FromValue(int value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var role = List()
            .SingleOrDefault(r => r.Value == value);

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(Role)} values: {string.Join(',', List().Select(r => r.Value))}. Provided value: {value}");

        return role;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, this))
            return true;

        if (obj is null || obj is not Role role)
            return false;

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
