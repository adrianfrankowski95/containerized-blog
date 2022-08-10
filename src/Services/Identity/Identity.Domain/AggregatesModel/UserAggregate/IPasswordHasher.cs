namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public interface IPasswordHasher
{
    public NonEmptyString HashPassword(Password password);
}
