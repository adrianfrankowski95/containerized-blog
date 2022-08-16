using Blog.Services.Blogging.Domain.SeedWork;
using Blog.Services.Blogging.Domain.Exceptions;

namespace Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

public sealed class User : Entity<UserId>
{
    public UserRole Role { get; }
    public string Name { get; }

    //ef core
    private User() { }

    public User(UserId id, string name, string roleName)
    {
        if (id == null)
            throw new BloggingDomainException($"{nameof(Id)} cannot be null");

        if (id.Value == Guid.Empty)
            throw new BloggingDomainException($"{nameof(Id)} cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new BloggingDomainException($"{nameof(Name)} cannot be null");

        UserRole role = UserRole.FromName(roleName);

        Id = id;
        Name = name;
        Role = role;
    }
}

public class UserId : ValueObject<UserId>
{
    public Guid Value { get; }

    public UserId(Guid userId) => Value = userId;

    public bool IsEmpty() => Value.Equals(Guid.Empty);

    protected override IEnumerable<object?> GetEqualityCheckAttributes()
    {
        yield return Value;
    }
}
