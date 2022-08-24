namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IPasswordHasher
{
    public PasswordHash HashPassword(Password password);
}
