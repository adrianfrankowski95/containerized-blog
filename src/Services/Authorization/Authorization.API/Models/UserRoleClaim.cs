using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Authorization.API.Models;

public sealed class UserRoleClaim : IdentityRoleClaim<int>
{
    private UserRoleClaim(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static readonly UserRoleClaim Reader = new(0, Constants.UserRoleClaimTypes.Reader);
    public static readonly UserRoleClaim Moderator = new(1, Constants.UserRoleClaimTypes.Moderator);
    public static readonly UserRoleClaim Blogger = new(2, Constants.UserRoleClaimTypes.Blogger);
    public static readonly UserRoleClaim Administrator = new(3, Constants.UserRoleClaimTypes.Administrator);

    public static UserRoleClaim GetDefault() => Reader;

    public static IEnumerable<UserRoleClaim> List() => new[] {
        Reader,
        Moderator,
        Blogger,
        Administrator };

    public static UserRoleClaim FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var role = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRoleClaim)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return role;
    }

    public static UserRoleClaim FromId(int id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        var role = List()
            .SingleOrDefault(r => r.Id == id);

        if (role is null)
            throw new InvalidOperationException($"Possible {nameof(UserRoleClaim)} IDs: {string.Join(',', List().Select(r => r.Id))}. Provided ID: {id}");

        return role;
    }
}
