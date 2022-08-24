using Blog.Services.Identity.Domain.SeedWork;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Permission : Enumeration
{
    private Permission(int value, NonEmptyString name) : base(value, name)
    {
    }

    public static readonly Permission CreateOtherUser = new(0, nameof(CreateOtherUser).ToLowerInvariant());
    public static readonly Permission ChangeOtherUsersPersonalData = new(1, nameof(ChangeOtherUsersPersonalData).ToLowerInvariant());
    public static readonly Permission DeleteOtherUser = new(2, nameof(DeleteOtherUser).ToLowerInvariant());
    public static readonly Permission SuspendOtherUser = new(3, nameof(SuspendOtherUser).ToLowerInvariant());
    public static readonly Permission ResetOtherUsersPassword = new(4, nameof(ResetOtherUsersPassword).ToLowerInvariant());
    public static readonly Permission ChangeOtherUsersRole = new(5, nameof(ChangeOtherUsersRole).ToLowerInvariant());

    public static IEnumerable<Permission> List() => new[] {
        CreateOtherUser,
        ChangeOtherUsersPersonalData,
        DeleteOtherUser,
        SuspendOtherUser,
        ResetOtherUsersPassword,
        ChangeOtherUsersRole };

    public static Permission FromName(NonEmptyString name)
    {
        var permission = List()
            .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

        if (permission is null)
            throw new InvalidOperationException($"Possible {nameof(Permission)} names: {string.Join(", ", List().Select(r => r.Name))}. Provided name: {name}");

        return permission;
    }

    public static Permission FromValue(int value)
    {
        var permission = List()
            .SingleOrDefault(r => r.Value == value);

        if (permission is null)
            throw new InvalidOperationException($"Possible {nameof(Permission)} values: {string.Join(',', List().Select(r => r.Value))}. Provided value: {value}");

        return permission;
    }
}
