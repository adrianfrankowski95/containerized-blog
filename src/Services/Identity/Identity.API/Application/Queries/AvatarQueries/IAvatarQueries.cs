using Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

namespace Blog.Services.Identity.API.Application.Queries.AvatarQueries;

public interface IAvatarQueries
{
    public Task<AvatarViewModel> GetAvatarByUsernameAsync(NonEmptyString username);
}
