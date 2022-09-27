using System.Data.Common;
using Blog.Services.Blogging.API.Application.Queries.TagQueries;
using Blog.Services.Blogging.API.Application.Queries.TagQueries.Models;
using Blog.Services.Blogging.API.Extensions;
using Blog.Services.Blogging.Domain.AggregatesModel.Shared;
using Dapper;

namespace Blog.Services.Blogging.API.Application.Queries.PostQueries;

public class DapperTagQueries : ITagQueries
{
    private readonly DbConnection _connection;

    public DapperTagQueries(DbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async IAsyncEnumerable<TagViewModel> GetTagsAsync()
    {
        string returnTagsQuery =
            @$"SELECT
                    a.id AS {nameof(TagViewModel.TagId)},
                    a.value AS {nameof(TagViewModel.Value)},
                    a.language {nameof(TagViewModel.Language)},
                    c.posts_count AS {nameof(TagViewModel.PostsCount)}
                FROM blogging.tags AS a
                LEFT JOIN(
                    SELECT COUNT(c0.post_id) AS posts_count, c0.tag_id
                    FROM(
                        SELECT t.post_id, pt.tag_id
                        FROM blogging.post_translations_tags AS pt
                        LEFT JOIN blogging.post_translations AS t ON t.id = pt.post_translation_id
                        GROUP BY t.post_id, pt.tag_id
                    ) AS c0
                    GROUP BY c0.tag_id
                ) AS c ON c.tag_id = a.id
                GROUP BY a.id, c.posts_count
                ORDER BY a.language, a.value ASC;";

        var tagsReader = await _connection.ExecuteReaderAsync(returnTagsQuery).ConfigureAwait(false);

        var rowParser = tagsReader.GetRowParser<TagViewModel>();

        while (await tagsReader.ReadAsync().ConfigureAwait(false))
            yield return rowParser(tagsReader);
    }

    public async IAsyncEnumerable<TagViewModel> GetTagsWithLanguageAsync(Language language)
    {
        string returnTagsQuery =
            @$"SELECT
                    a.id AS {nameof(TagViewModel.TagId)},
                    a.value AS {nameof(TagViewModel.Value)},
                    a.language {nameof(TagViewModel.Language)},
                    c.posts_count AS {nameof(TagViewModel.PostsCount)}
                FROM blogging.tags AS a
                LEFT JOIN(
                    SELECT COUNT(c0.post_id) AS posts_count, c0.tag_id
                    FROM(
                        SELECT t.post_id, pt.tag_id
                        FROM blogging.post_translations_tags AS pt
                        LEFT JOIN blogging.post_translations AS t ON t.id = pt.post_translation_id
                        GROUP BY t.post_id, pt.tag_id
                    ) AS c0
                    GROUP BY c0.tag_id
                ) AS c ON c.tag_id = a.id
                WHERE a.language = @Language
                GROUP BY a.id, c.posts_count
                ORDER BY a.value ASC;";

        var parameters = new { Language = language.Name };

        var tagsReader = await _connection.ExecuteReaderAsync(returnTagsQuery, parameters).ConfigureAwait(false);

        var rowParser = tagsReader.GetRowParser<TagViewModel>();

        while (await tagsReader.ReadAsync().ConfigureAwait(false))
            yield return rowParser(tagsReader);
    }
}