using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Authorization.API.Models;

public sealed class UserRole : IdentityRole<int>
{
    private UserRole(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static readonly UserRole Reader = new(0, Constants.UserRoleTypes.Reader);
    public static readonly UserRole Moderator = new(1, Constants.UserRoleTypes.Moderator);
    public static readonly UserRole Blogger = new(2, Constants.UserRoleTypes.Blogger);
    public static readonly UserRole Administrator = new(3, Constants.UserRoleTypes.Administrator);

    public static UserRole GetDefault() => Reader;

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

    public static UserRole FromId(int id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        var role = List()
            .SingleOrDefault(r => r.Id == id);

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRole)} IDs: {string.Join(',', List().Select(r => r.Id))}. Provided ID: {id}");

        return role;
    }
}
