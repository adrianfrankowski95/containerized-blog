namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public static class UsernameRequirementsFactory
{
    public static IEnumerable<IRequirement<Username>> DefaultSet(IUserRepository users)
        => new IRequirement<Username>[]
        {
            new UsernameMustBeMin3CharactersLong(),
            new UsernameMustBeMax32CharactersLong(),
            new UsernameMustBeUnique(users)
        };
}

public class UsernameMustBeMin3CharactersLong : IRequirement<Username>
{
    public RequirementMessage<Username> Message => "Username must be at least 3 characters long.";
    public bool IsSatisfiedBy(Username username) => username.Length >= 3;
}

public class UsernameMustBeMax32CharactersLong : IRequirement<Username>
{
    public RequirementMessage<Username> Message => "Username must be at most 32 characters long.";
    public bool IsSatisfiedBy(Username username) => username.Length > 32;
}

public class UsernameMustBeUnique : IRequirement<Username>
{
    private readonly IUserRepository _users;
    public RequirementMessage<Username> Message => "Username is already in use.";
    public bool IsSatisfiedBy(Username username) => !_users.IsInUse(username);

    public UsernameMustBeUnique(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }
}