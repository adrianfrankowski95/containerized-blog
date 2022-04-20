using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;

namespace Blog.Services.Blogging.API.Infrastructure.Services;

public interface IIdentityService
{
    public bool TryGetAuthenticatedUser(out User user);
}
