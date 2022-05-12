using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Authorization.API.Models;

public sealed class Role : IdentityRole<Guid>
{
    public int Value { get; }

    private Role(int value, string name)
    {
        Id = Guid.NewGuid();

        Value = value;
        Name = name;
    }

    public static readonly Role Reader = new(0, Constants.UserRoleTypes.Reader);
    public static readonly Role Moderator = new(1, Constants.UserRoleTypes.Moderator);
    public static readonly Role Blogger = new(2, Constants.UserRoleTypes.Blogger);
    public static readonly Role Administrator = new(3, Constants.UserRoleTypes.Administrator);

    public static Role GetDefault() => Reader;

    public static IEnumerable<Role> List() => new[] {
        Reader,
        Moderator,
        Blogger,
        Administrator };

    public static Role FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

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
}
