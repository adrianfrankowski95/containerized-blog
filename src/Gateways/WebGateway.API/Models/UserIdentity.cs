namespace Blog.Gateways.WebGateway.API.Models;

public record UserIdentity
{
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string UserName { get; init; }
    public string UserRole { get; init; }
    public bool IsPersistent { get; init; }

    public UserIdentity(Guid userId, string email, string userName, string userRole, bool isPersistent)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        UserRole = userRole;
        IsPersistent = isPersistent;
    }
}

