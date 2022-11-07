
using Blog.Services.Identity.API.Application.Queries.AvatarQueries.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Application.Queries.AvatarQueries;

public interface IAvatarQueries
{
    public Task<AvatarViewModel> GetAvatarByUsernameAsync(NonEmptyString username);
}
