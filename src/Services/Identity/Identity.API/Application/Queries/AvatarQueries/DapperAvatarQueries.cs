using System.Data.Common;
using Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Infrastructure;
using Dapper;

namespace Blog.Services.Identity.API.Application.Queries.AvatarQueries;

public class DapperAvatarQueries : IAvatarQueries
{
    private readonly DbConnection _connection;

    public DapperAvatarQueries(DbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public Task<AvatarViewModel> GetAvatarByUsernameAsync(NonEmptyString username)
    {
        string returnAvatarQuery =
            @$"SELECT
                    a.image_data AS {nameof(AvatarViewModel.ImageData)},
                    a.format AS {nameof(AvatarViewModel.Format)}
                FROM {IdentityDbContext.DefaultSchema}.avatars AS a
                    INNER JOIN (SELECT u.id, u.username FROM {IdentityDbContext.DefaultSchema}.users AS u)
                    AS usr ON (usr.username = @Username)
	            WHERE a.user_id = usr.id";

        var parameters = new { Username = username };

        return _connection.QueryFirstOrDefaultAsync<AvatarViewModel>(returnAvatarQuery, parameters);
    }
}